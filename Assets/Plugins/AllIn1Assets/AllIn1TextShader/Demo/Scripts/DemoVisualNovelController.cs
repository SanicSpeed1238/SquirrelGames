using UnityEngine;
using UnityEngine.UI;

namespace AllIn1Assets.AllIn1TextShader
{
	public class DemoVisualNovelController : TextShaderDemoElement
	{
		private int currentTextIndex;
		private float timer;

		public Image imgCharacter;
		public DemoVisualNovelEntry[] entries;

		void Start()
		{
			currentTextIndex = 0;
			timer = 0f;

			ShowCurrentText();
		}

		protected override void Update()
		{
			base.Update();

			timer += Time.deltaTime;

			float timeToNextMessage = entries[currentTextIndex].delayTime;
			if(timer >= timeToNextMessage)
			{
				NextText();

				timer = 0f;
			} 
		}

		private void NextText()
		{
			IncrementTextIndex();
			ShowCurrentText();
		}

		private void IncrementTextIndex()
		{
			currentTextIndex++;

			if (currentTextIndex >= entries.Length)
			{
				currentTextIndex = 0;
			}
		}

		//Carefully here, show is completly overriden and base.show() is not called
		public override void Show(DemoMovementDir dir)
		{
			if (!gameObject.activeSelf)
			{
				gameObject.SetActive(true);
			}

			currentTextIndex = 0;
			ShowCurrentText();

			if (moveAwayOnHide)
			{
				PrepareBeforeShow(dir);
				StartTransitionIn();
			}

			state = State.SHOWING;
		}

		private void ShowCurrentText()
		{
			DemoVisualNovelEntry currentEntry = entries[currentTextIndex];

			imgCharacter.sprite = currentEntry.spCharacter;

			for (int i = 0; i < typewriters.Length; i++)
			{
				typewriters[i].SetupByEntry(currentEntry.typewriterConfig);

				if (currentTextIndex == 2)
				{
					typewriters[i].RevealImmediate();
				}
				else
				{
					typewriters[i].Reveal();
				}
			}

			for (int i = 0; i < materialConfigurators.Length; i++) 
			{
				materialConfigurators[i].UpdateConfiguration(true);
			}

			for (int i = 0; i < typewriters.Length; i++)
			{
				typewriters[i].Update();
			}
		}
	}
}