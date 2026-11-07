using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace AllIn1Assets.AllIn1TextShader
{
	[AddComponentMenu("AllIn1TextShader/Typewriter")]
	public class AllIn1TextTypewriter : MonoBehaviour
	{
		public enum PlaybackState
		{
			STOPPED,
			PLAYING,
			PAUSED,
		}

		// Values 0 and 1 match the old revealOnStart bool so scenes saved with it keep their setting
		public enum StartAction
		{
			[InspectorName("Nothing")] NOTHING = 0,
			[InspectorName("Reveal")] REVEAL = 1,
			[InspectorName("Show Immediately")] SHOW_IMMEDIATELY = 2,
		}

		public MeshEffectCollection effectCollection;

		public EasingFunctionsSO easeFunctionCollectionLooped;

		public MeshEffectGroupConfig revealConfig;
		public MeshEffectGroupConfig loopedConfig;
		public MeshEffectGroupConfig hideConfigRuntime;

		public TMP_Text textComponent;

		public MeshEffectsDispatcher meshEffectDispatcher;

		[FormerlySerializedAs("revealOnStart")]
		public StartAction onStart = StartAction.REVEAL;
		public bool useRevealAsHideConfig = true;
		public string rawText;


		[SerializeField] private MeshEffectGroupConfig hideConfigSelected;


		public bool lastShowTags;
		public bool showTags;

		public TextLayeredComponent textLayeredComponent;

		public UnityEvent onRevealStartedEvent = new UnityEvent();
		public UnityEvent onRevealCompletedEvent = new UnityEvent();
		public UnityEvent<char, int> onCharacterRevealStartedEvent = new UnityEvent<char, int>();
		public UnityEvent onHideStartedEvent = new UnityEvent();
		public UnityEvent onHideCompletedEvent = new UnityEvent();

		public event System.Action OnRevealStarted;
		public event System.Action OnRevealCompleted;
		public event System.Action<char, int> OnCharacterRevealStarted;
		public event System.Action OnHideStarted;
		public event System.Action OnHideCompleted;

#if UNITY_EDITOR
		public bool revealConfigUnfolded;
		public bool loopedConfigUnfolded;
		public bool hideConfigUnfolded;
		public bool eventsUnfolded;
		
		public float previewTime;

		public PreviewMode previewMode;
		public float previewSliderValue;
#endif

		private TextMeshHandler meshHandler;
		private List<TextParsedGroup> parsedGroups;
		private PlaybackState playbackState;
		private bool effectsApplied;
		// A stopped typewriter writes its mesh once and then leaves it alone until something changes
		private bool stoppedPoseWritten;

		// The one-shot whose Completed event is still owed
		private enum PendingRun { NONE, REVEAL, HIDE }
		private PendingRun pendingRun;
		// The pending run's Started waits for the next Update, like every event but HideImmediate's
		private bool startPending;
		// Built but not played yet, so the next Play() starts a reveal
		private bool revealArmed;
		private readonly List<int> revealStartedGroups = new List<int>();
		private int characterSearchHint;

		private void OnEnable()
		{
			Init();
		}

		private void OnDisable()
		{
			if (meshHandler != null && meshHandler.reapplyEffects == (System.Action)ReapplyEffects)
			{
				meshHandler.reapplyEffects = null;
			}
		}

		// Called by the mesh handler right after it snapshots a TMP relayout (spacing, size, alignment...)
		private void ReapplyEffects()
		{
			if (meshEffectDispatcher == null) { return; }

#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				if (IsInPreviewMode())
				{
					meshEffectDispatcher.ReapplyPreview(previewMode);
				}
				return;
			}
#endif

			if (effectsApplied)
			{
				meshEffectDispatcher.Reapply();
			}
		}

		private void Start()
		{
			switch (onStart)
			{
				case StartAction.REVEAL:
					Play();
					break;
				case StartAction.SHOW_IMMEDIATELY:
					RevealImmediate();
					break;
			}
		}

		private void InitMeshHandler()
		{
			int instanceID = gameObject.GetInstanceID();
			meshHandler = TextMeshHandlerRegistry.GetOrCreateTextMeshHandler(instanceID, textComponent);

			if(textLayeredComponent != null)
			{
				textLayeredComponent.InitMeshHandlers();
			}

			meshHandler.reapplyEffects = ReapplyEffects;
			effectsApplied = false;

			meshHandler.RebuildCache();
		}

		public void Init()
		{
			InitMeshHandler();

			string textToParse = GetTextToParse();

			if (Application.isPlaying)
			{
				string cleanText;
				this.parsedGroups = ParseText(textToParse, out cleanText);
				textComponent.text = cleanText;
			}
			else
			{
#if UNITY_EDITOR
				if(previewMode == PreviewMode.NONE)
				{
					this.parsedGroups = new List<TextParsedGroup>();
				}
				else
				{
					string cleanText;
					this.parsedGroups = ParseText(textToParse, out cleanText);
					textComponent.text = cleanText;
				}
#endif
			}

			ConfigureHideConfigRuntime();

			playbackState = PlaybackState.STOPPED;
			stoppedPoseWritten = false;

			meshEffectDispatcher = new MeshEffectsDispatcher(meshHandler, revealConfig, loopedConfig, hideConfigRuntime);
			meshEffectDispatcher.RecordRevealStartedGroups(revealStartedGroups);
			meshEffectDispatcher.Build(parsedGroups);
			ArmReveal();
		}

		private List<TextParsedGroup> ParseText(string text, out string cleanText)
		{
			List<TextParsedGroup> res = new List<TextParsedGroup>();
			cleanText = MeshEffectsParser.Parse(text, effectCollection, res, easeFunctionCollectionLooped);

			return res;
		}

		public void ConfigureHideConfigRuntime()
		{
			if (useRevealAsHideConfig && revealConfig != null)
			{
				hideConfigRuntime = ScriptableObject.CreateInstance<MeshEffectGroupConfig>();
				hideConfigRuntime.CopyValuesFrom(revealConfig);
				hideConfigRuntime.Invert();
			}
			else
			{
				hideConfigRuntime = hideConfigSelected;
			}
		}

		public void Update()
		{
			UpdateTypewriter(Time.deltaTime);
			if (playbackState != PlaybackState.STOPPED || !stoppedPoseWritten)
			{
				WriteChangesToMesh();
				stoppedPoseWritten = playbackState == PlaybackState.STOPPED;
			}
			DispatchEvents();
		}

		// True while Update writes the mesh every frame, so the spline knows whether its own write is needed
		public bool IsWritingEveryFrame()
		{
			return isActiveAndEnabled && playbackState != PlaybackState.STOPPED;
		}

		public void WriteChangesToMesh()
		{
			meshHandler.WriteChanges();
		}

		public void UpdateTypewriter(float deltaTime)
		{
			switch (playbackState)
			{
				case PlaybackState.PLAYING:
				case PlaybackState.PAUSED:
					meshEffectDispatcher.Update(deltaTime);
					effectsApplied = true;
					break;
			}
		}

		public string GetTextToParse()
		{
			string res = rawText;
			if (showTags)
			{
				res = textComponent.text;
			}

			return res;
		}

		public void SaveRawText()
		{
			if (showTags)
			{
				rawText = textComponent.text;
			}
		}

		public void RetrieveRawText()
		{
			if (showTags)
			{
				textComponent.text = rawText;
			}
		}

		#region PLUBLIC METHODS
		// Restores the clock Pause() stopped, and starts an armed reveal from its first pose (after Init, SetupByEntry, HideImmediate or a new text)
		public void Play()
		{
			meshEffectDispatcher.Resume();
			playbackState = PlaybackState.PLAYING;
			if (revealArmed)
			{
				meshEffectDispatcher.Reveal();
				BeginRun(PendingRun.REVEAL);
			}
		}

		public void Pause()
		{
			playbackState = PlaybackState.PAUSED;
			meshEffectDispatcher.Pause();
		}

		public void Resume()
		{
			if(playbackState == PlaybackState.PAUSED)
			{
				playbackState = PlaybackState.PLAYING;

				meshEffectDispatcher.Resume();
			}
		}

		public void Stop()
		{
			playbackState = PlaybackState.STOPPED;
		}

		public void Reveal()
		{
			meshEffectDispatcher.Reveal();
			playbackState = PlaybackState.PLAYING;
			BeginRun(PendingRun.REVEAL);
		}

		public void RevealImmediate()
		{
			meshEffectDispatcher.RevealImmediate();
			playbackState = PlaybackState.PLAYING;
			BeginRun(PendingRun.REVEAL);
		}

		public void Hide()
		{
			meshEffectDispatcher.Hide();
			playbackState = PlaybackState.PLAYING;
			BeginRun(PendingRun.HIDE);
		}

		// Reveal and hide scrubbed from outside, used by the spline so the travel sets the timing. Only the first call of a scrub starts a run
		public void RevealAtProgress(float normalizedProgress)
		{
			bool continuing = meshEffectDispatcher.IsExternalProgressActive(VisibilityEffectType.REVEAL);
			meshEffectDispatcher.RevealAtProgress(normalizedProgress);
			playbackState = PlaybackState.PLAYING;
			if (!continuing) { BeginRun(PendingRun.REVEAL); }
		}

		public void HideAtProgress(float normalizedProgress)
		{
			bool continuing = meshEffectDispatcher.IsExternalProgressActive(VisibilityEffectType.HIDE);
			meshEffectDispatcher.HideAtProgress(normalizedProgress);
			playbackState = PlaybackState.PLAYING;
			if (!continuing) { BeginRun(PendingRun.HIDE); }
		}

		public void RevealAtProgress(float[] quadProgress)
		{
			bool continuing = meshEffectDispatcher.IsExternalProgressActive(VisibilityEffectType.REVEAL);
			meshEffectDispatcher.RevealAtProgress(quadProgress);
			playbackState = PlaybackState.PLAYING;
			if (!continuing) { BeginRun(PendingRun.REVEAL); }
		}

		public void HideAtProgress(float[] quadProgress)
		{
			bool continuing = meshEffectDispatcher.IsExternalProgressActive(VisibilityEffectType.HIDE);
			meshEffectDispatcher.HideAtProgress(quadProgress);
			playbackState = PlaybackState.PLAYING;
			if (!continuing) { BeginRun(PendingRun.HIDE); }
		}

		public float GetRevealProgressNormalized()
		{
			float res = meshEffectDispatcher.GetRevealProgressNormalized();
			return res;
		}

		public float GetHideProgressNormalized()
		{
			float res = meshEffectDispatcher.GetHideProgressNormalized();
			return res;
		}

		public void FinishProgressControl()
		{
			meshEffectDispatcher.FinishExternalProgress();
		}

		// The hide preset's last pose at once; the next Play() reveals again from the start
		public void HideImmediate()
		{
			meshEffectDispatcher.HideImmediate();

			meshEffectDispatcher.Update(0f);
			effectsApplied = true;
			WriteChangesToMesh();

			playbackState = PlaybackState.STOPPED;

			// Fired here rather than from Update: the hide is already done, and pooling code often deactivates the object right after
			ArmReveal();
			if (Application.isPlaying)
			{
				Fire(onHideStartedEvent, OnHideStarted);
				Fire(onHideCompletedEvent, OnHideCompleted);
			}
		}

		#endregion

		#region EVENTS
		private void ArmReveal()
		{
			revealArmed = true;
			pendingRun = PendingRun.NONE;
			startPending = false;
			revealStartedGroups.Clear();
		}

		// A rebuilt text restarts its reveal: a run if the typewriter is going, armed for Play() if it is stopped
		private void RestartReveal()
		{
			if (playbackState == PlaybackState.STOPPED) { ArmReveal(); }
			else { BeginRun(PendingRun.REVEAL); }
		}

		// A run replaced before its Started fired (Hide then Reveal in one frame) fires nothing
		private void BeginRun(PendingRun run)
		{
			pendingRun = run;
			startPending = true;
			revealArmed = false;
			revealStartedGroups.Clear();
		}

		// Runs after the mesh is written, so a listener can restart the typewriter or change the text without breaking the frame.
		// Starting events here too means a script that subscribes in its own Start never misses the On Start reveal
		private void DispatchEvents()
		{
			if (!Application.isPlaying)
			{
				startPending = false;
				revealStartedGroups.Clear();
				return;
			}

			if (startPending)
			{
				startPending = false;
				if (pendingRun == PendingRun.REVEAL) { Fire(onRevealStartedEvent, OnRevealStarted); }
				else if (pendingRun == PendingRun.HIDE) { Fire(onHideStartedEvent, OnHideStarted); }
			}

			if (revealStartedGroups.Count > 0)
			{
				FireCharacterEvents();
				revealStartedGroups.Clear();
			}

			// A run a listener just started fires its Started first, on the next Update
			if (pendingRun == PendingRun.NONE || startPending) { return; }

			PendingRun run = pendingRun;
			bool complete = run == PendingRun.REVEAL ? meshEffectDispatcher.IsRevealComplete() : meshEffectDispatcher.IsHideComplete();
			if (!complete) { return; }

			pendingRun = PendingRun.NONE;
			if (run == PendingRun.REVEAL) { Fire(onRevealCompletedEvent, OnRevealCompleted); }
			else { Fire(onHideCompletedEvent, OnHideCompleted); }
		}

		private void FireCharacterEvents()
		{
			TMP_TextInfo textInfo = textComponent.textInfo;
			if (textInfo == null || textInfo.characterInfo == null) { return; }

			for (int i = 0; i < revealStartedGroups.Count; i++)
			{
				int characterIndex = FindCharacterByVertex(textInfo, meshEffectDispatcher.GetRevealGroupStartVertex(revealStartedGroups[i]));
				if (characterIndex < 0) { continue; }

				char character = textInfo.characterInfo[characterIndex].character;
				onCharacterRevealStartedEvent.Invoke(character, characterIndex);
				if (OnCharacterRevealStarted != null) { OnCharacterRevealStarted(character, characterIndex); }
			}
		}

		// Groups know their first vertex, not their character. Reveals mostly run in order, so the search starts at the last hit
		private int FindCharacterByVertex(TMP_TextInfo textInfo, int vertexIndex)
		{
			int count = Mathf.Min(textInfo.characterCount, textInfo.characterInfo.Length);
			for (int n = 0; n < count; n++)
			{
				int i = (characterSearchHint + n) % count;
				ref TMP_CharacterInfo characterInfo = ref textInfo.characterInfo[i];
				if (characterInfo.isVisible && characterInfo.materialReferenceIndex == 0 && characterInfo.vertexIndex == vertexIndex)
				{
					characterSearchHint = i;
					return i;
				}
			}

			return -1;
		}

		private static void Fire(UnityEvent unityEvent, System.Action action)
		{
			if (unityEvent != null) { unityEvent.Invoke(); }
			if (action != null) { action(); }
		}
		#endregion


		public PlaybackState GetPlaybackState()
		{
			return playbackState;
		}

		public bool IsRevealed()
		{
			bool res = meshEffectDispatcher.IsRevealed();
			return res;
		}

		public bool IsHidden()
		{
			bool res = meshEffectDispatcher.IsHidden();
			return res;
		}

		public void SetText(string text)
		{
			string cleanText;
			this.parsedGroups = ParseText(text, out cleanText);
			this.textComponent.text = cleanText;
			stoppedPoseWritten = false;

			meshEffectDispatcher.Build(parsedGroups);
			meshEffectDispatcher.Reveal();
			RestartReveal();

			// Build's relayout uploaded the new letters in the previous text's state, which showed them whole for a frame
			if (playbackState != PlaybackState.STOPPED)
			{
				UpdateTypewriter(0f);
				WriteChangesToMesh();
			}
		}

		public void SetupByEntry(TypewriterEntry entry)
		{
			string cleanText;
			this.parsedGroups = ParseText(entry.text, out cleanText);

			this.textComponent.text = cleanText;

			this.textComponent.fontSize = entry.fontSize;

			SetAlignment(entry.horizontalAlignment, entry.verticalAlignment);
			SetEffectConfigurations(entry.revealConfig, entry.loopedConfig, entry.hideConfig);

			meshEffectDispatcher.SetEffectGroupConfigurations(revealConfig, loopedConfig, hideConfigRuntime);
			meshEffectDispatcher.Build(parsedGroups);
			ArmReveal();
			stoppedPoseWritten = false;
		}

		public void Refresh()
		{
			stoppedPoseWritten = false;
			meshEffectDispatcher.SetEffectGroupConfigurations(revealConfig, loopedConfig, hideConfigRuntime);
			meshEffectDispatcher.Build(parsedGroups);
			meshEffectDispatcher.Reveal();
			RestartReveal();
		}

		public void SetEffectConfigurations(MeshEffectGroupConfig revealConfig, MeshEffectGroupConfig loopConfig, MeshEffectGroupConfig hideConfig)
		{
			if (revealConfig != null)
			{
				this.revealConfig = revealConfig;
			}

			if (loopConfig != null)
			{
				this.loopedConfig = loopConfig;
			}

			if (hideConfig != null)
			{
				this.hideConfigRuntime = hideConfig;
			}
		}

		public void SetAlignment(TypewriterEntry.HorizontalAlignment horizontalAlignment, TypewriterEntry.VerticalAlignment verticalAlignment)
		{
			switch (horizontalAlignment)
			{
				case TypewriterEntry.HorizontalAlignment.LEFT:
					this.textComponent.horizontalAlignment = HorizontalAlignmentOptions.Left;
					break;
				case TypewriterEntry.HorizontalAlignment.CENTER:
					this.textComponent.horizontalAlignment = HorizontalAlignmentOptions.Center;
					break;
				case TypewriterEntry.HorizontalAlignment.RIGHT:
					this.textComponent.horizontalAlignment = HorizontalAlignmentOptions.Right;
					break;
			}

			switch (verticalAlignment)
			{
				case TypewriterEntry.VerticalAlignment.BOTTOM:
					this.textComponent.verticalAlignment = VerticalAlignmentOptions.Bottom;
					break;
				case TypewriterEntry.VerticalAlignment.MIDDLE:
					this.textComponent.verticalAlignment = VerticalAlignmentOptions.Middle;
					break;
				case TypewriterEntry.VerticalAlignment.TOP:
					this.textComponent.verticalAlignment = VerticalAlignmentOptions.Top;
					break;
			}
		}

#if UNITY_EDITOR
		public void UpdateEditor(float editorDeltaTime)
		{
			if (IsInPreviewMode())
			{
				UpdateTypewriterPreview(editorDeltaTime);
				WriteChangesToMesh();
			}
		}

		public void SetupReferences(TMP_Text textComponent,
			MeshEffectCollection effectCollection,
			MeshEffectGroupConfig defaultRevealConfig, MeshEffectGroupConfig defaultLoopedConfig, MeshEffectGroupConfig defaultHideConfig, 
			EasingFunctionsSO easeFunctionCollectionLooped)
		{
			this.effectCollection = effectCollection;

			this.textComponent = textComponent;
			
			this.revealConfig = defaultRevealConfig;
			this.loopedConfig = defaultLoopedConfig;
			this.hideConfigSelected = defaultHideConfig;

			this.easeFunctionCollectionLooped = easeFunctionCollectionLooped;

			this.rawText = textComponent.text;

			this.textLayeredComponent = gameObject.GetComponent<TextLayeredComponent>();
		}

		public MeshEffectGroupConfig GetHideConfigSelectedFomEditor()
		{
			return hideConfigSelected;
		}

		public void SetHideConfigSelectedFromEditor(MeshEffectGroupConfig hideConfigSelected)
		{
			this.hideConfigSelected = hideConfigSelected;
		}

		public bool IsInPreviewMode()
		{
			bool res = previewMode != PreviewMode.NONE;
			return res;
		}

		public void UpdateTypewriterPreview(float deltaTime)
		{
			if (meshEffectDispatcher == null)
			{
				meshEffectDispatcher = new MeshEffectsDispatcher(meshHandler, revealConfig, loopedConfig, hideConfigRuntime);
				meshEffectDispatcher.Build(parsedGroups);
			}

			meshEffectDispatcher.UpdatePreview(previewSliderValue, deltaTime, previewMode);
		}

		public void PreviewModeChanged()
		{
			if (previewMode == PreviewMode.NONE)
			{
				showTags = lastShowTags;

				RetrieveRawText();
			}
			else
			{
				SaveRawText();

				lastShowTags = showTags;
				showTags = false;
			}
		}

		//Returns if some reference has been changed
		public bool RefreshTMPReferenceIfNeeded()
		{
			bool res = false;

			if (textComponent == null)
			{
				textComponent = gameObject.GetComponent<TMP_Text>();
				this.rawText = textComponent.text;

				res = true;
			}

			return res;
		}

		public bool RefreshReferencesIfNeeded(MeshEffectConfigCollection configCollectionReveal,
			MeshEffectConfigCollection configCollectionLoop, MeshEffectConfigCollection configCollectionHide, 
			string effectCollectionDataPath, string easeFunctionCollectionLoopedPath)
		{
			bool res = false;

			res = RefreshTMPReferenceIfNeeded() || res;
			res = RefreshMeshEffectGroupCofigsIfNeeded(configCollectionReveal, configCollectionLoop, configCollectionHide) || res;
			res = RefreshEffectCollectionIfNeeded(effectCollectionDataPath) || res;
			res = RefreshTextLayeredComponentIfNeeded() || res;
			res = RefreshEaseFunctionCollectionLoopedIfNeeded(easeFunctionCollectionLoopedPath) || res;

			return res;
		}

		public bool RefreshMeshEffectGroupCofigsIfNeeded(MeshEffectConfigCollection configCollectionReveal,
			MeshEffectConfigCollection configCollectionLoop, MeshEffectConfigCollection configCollectionHide)
		{
			bool res = false;

			if (revealConfig == null)
			{
				this.revealConfig = configCollectionReveal.configs[0];
				res = true;
			}

			if (loopedConfig == null)
			{
				this.loopedConfig = configCollectionLoop.configs[0];
				res = true;
			}

			if (hideConfigSelected == null)
			{
				this.hideConfigSelected = configCollectionHide.configs[0];
				res = true;
			}

			return res;
		}

		public bool RefreshEffectCollectionIfNeeded(string effectCollectionDataPath)
		{
			bool res = false;

			if(this.effectCollection == null)
			{
				effectCollection = UnityEditor.AssetDatabase.LoadAssetAtPath<MeshEffectCollection>(effectCollectionDataPath);
				res = true;
			}

			return res;
		}

		public bool RefreshTextLayeredComponentIfNeeded()
		{
			bool res = false;

			if(textLayeredComponent == null)
			{
				textLayeredComponent = gameObject.GetComponent<TextLayeredComponent>();
				res = true;
			}

			return res;
		}

		public bool RefreshEaseFunctionCollectionLoopedIfNeeded(string path)
		{
			bool res = false;

			if (easeFunctionCollectionLooped == null)
			{
				easeFunctionCollectionLooped = UnityEditor.AssetDatabase.LoadAssetAtPath<EasingFunctionsSO>(path);
				res = true;
			}

			return res;
		}
#endif
	}
}