using System.Collections.Generic;
using UnityEngine;

namespace AllIn1Assets.AllIn1TextShader
{
	// 2D cubic Bézier through a list of knots, with an arc-length table so positions along the curve are even
	public class SplinePath
	{
		private const int MIN_SAMPLES_PER_SEGMENT = 2;
		private const float MIN_TANGENT_SQR_MAGNITUDE = 1e-10f;
		private const int SPAN_SOLVE_MAX_ITERATIONS = 8;
		private const float SPAN_SOLVE_TOLERANCE = 1e-4f;
		private const int CLOSEST_REFINE_STEPS = 12;
		private const int CLOSEST_REFINE_PASSES = 3;

		private SplineKnot[] knots;
		private bool closed;
		private int samplesPerSegment;

		private float[] cumulativeLengths;
		private int segmentCount;
		private float totalLength;

		public bool IsValid { get { return segmentCount > 0 && totalLength > 0f; } }
		public bool Closed { get { return closed; } }
		public float TotalLength { get { return totalLength; } }
		public int SegmentCount { get { return segmentCount; } }
		public int SamplesPerSegment { get { return samplesPerSegment; } }

		public SplinePath()
		{
			knots = new SplineKnot[0];
			cumulativeLengths = new float[1];
		}

		public void Rebuild(IList<SplineKnot> newKnots, bool closed, int samplesPerSegment)
		{
			this.closed = closed;
			this.samplesPerSegment = Mathf.Max(MIN_SAMPLES_PER_SEGMENT, samplesPerSegment);

			int knotCount = newKnots != null ? newKnots.Count : 0;
			if (knots.Length != knotCount)
			{
				knots = new SplineKnot[knotCount];
			}
			for (int i = 0; i < knotCount; i++)
			{
				knots[i] = newKnots[i];
			}

			segmentCount = 0;
			if (knotCount >= 2)
			{
				segmentCount = closed ? knotCount : knotCount - 1;
			}

			BuildArcLengthTable();
		}

		private void BuildArcLengthTable()
		{
			int sampleCount = segmentCount * samplesPerSegment + 1;
			if (cumulativeLengths.Length != sampleCount)
			{
				cumulativeLengths = new float[sampleCount];
			}

			cumulativeLengths[0] = 0f;
			totalLength = 0f;

			if (segmentCount == 0) { return; }

			Vector2 previous = EvaluateSegment(0, 0f);
			int sampleIdx = 1;
			for (int segment = 0; segment < segmentCount; segment++)
			{
				for (int step = 1; step <= samplesPerSegment; step++)
				{
					float t = (float)step / samplesPerSegment;
					Vector2 current = EvaluateSegment(segment, t);
					totalLength += Vector2.Distance(previous, current);
					cumulativeLengths[sampleIdx] = totalLength;
					previous = current;
					sampleIdx++;
				}
			}
		}

		public Vector2 EvaluateSegment(int segment, float t)
		{
			GetSegmentControlPoints(segment, out Vector2 p0, out Vector2 p1, out Vector2 p2, out Vector2 p3);

			float u = 1f - t;
			float uu = u * u;
			float tt = t * t;

			Vector2 res = (uu * u) * p0 + (3f * uu * t) * p1 + (3f * u * tt) * p2 + (tt * t) * p3;
			return res;
		}

		public Vector2 EvaluateSegmentTangent(int segment, float t)
		{
			GetSegmentControlPoints(segment, out Vector2 p0, out Vector2 p1, out Vector2 p2, out Vector2 p3);

			float u = 1f - t;

			Vector2 derivative = (3f * u * u) * (p1 - p0) + (6f * u * t) * (p2 - p1) + (3f * t * t) * (p3 - p2);
			if (derivative.sqrMagnitude < MIN_TANGENT_SQR_MAGNITUDE)
			{
				// Degenerate handle (a knot sitting on its own control point): fall back to the chord
				derivative = p3 - p0;
			}

			Vector2 res = derivative.sqrMagnitude > MIN_TANGENT_SQR_MAGNITUDE ? derivative.normalized : Vector2.right;
			return res;
		}

		private void GetSegmentControlPoints(int segment, out Vector2 p0, out Vector2 p1, out Vector2 p2, out Vector2 p3)
		{
			SplineKnot from = knots[segment];
			SplineKnot to = knots[(segment + 1) % knots.Length];

			p0 = from.position;
			p1 = from.position + from.tangentOut;
			p2 = to.position + to.tangentIn;
			p3 = to.position;
		}

		// Maps an arc-length distance to a position and unit tangent. Closed splines wrap, open ones clamp
		public void SampleByArcLength(float s, out Vector2 position, out Vector2 tangent)
		{
			if (!IsValid)
			{
				position = Vector2.zero;
				tangent = Vector2.right;
				return;
			}

			s = WrapOrClampArcLength(s);

			int sampleIdx = FindSampleIndex(s);
			float spanStart = cumulativeLengths[sampleIdx];
			float span = cumulativeLengths[sampleIdx + 1] - spanStart;

			int segment = sampleIdx / samplesPerSegment;
			int step = sampleIdx % samplesPerSegment;
			float tStart = (float)step / samplesPerSegment;
			float tEnd = (float)(step + 1) / samplesPerSegment;
			float t = span > 0f ? SolveSpanParameter(segment, tStart, tEnd, Mathf.Clamp(s - spanStart, 0f, span), span) : tStart;

			position = EvaluateSegment(segment, t);
			tangent = EvaluateSegmentTangent(segment, t);
		}

		// t whose chord from the span start equals the distance (the table's own measure); a linear guess alone drifts where the curve speeds up
		private float SolveSpanParameter(int segment, float tLow, float tHigh, float distance, float span)
		{
			Vector2 spanStartPoint = EvaluateSegment(segment, tLow);
			float errorLow = -distance;
			float errorHigh = span - distance;
			float tolerance = span * SPAN_SOLVE_TOLERANCE;
			int lastMovedSide = 0;

			float res = tLow;
			for (int i = 0; i < SPAN_SOLVE_MAX_ITERATIONS; i++)
			{
				res = tLow + (tHigh - tLow) * errorLow / (errorLow - errorHigh);
				float error = Vector2.Distance(spanStartPoint, EvaluateSegment(segment, res)) - distance;
				if (Mathf.Abs(error) <= tolerance) { break; }

				if (error < 0f)
				{
					tLow = res;
					errorLow = error;
					if (lastMovedSide < 0) { errorHigh *= 0.5f; }
					lastMovedSide = -1;
				}
				else
				{
					tHigh = res;
					errorHigh = error;
					if (lastMovedSide > 0) { errorLow *= 0.5f; }
					lastMovedSide = 1;
				}
			}

			return res;
		}

		public void SampleNormalized(float normalized, out Vector2 position, out Vector2 tangent)
		{
			SampleByArcLength(normalized * totalLength, out position, out tangent);
		}

		public float WrapOrClampArcLength(float s)
		{
			float res = s;
			if (closed)
			{
				res = Mathf.Repeat(s, totalLength);
			}
			else
			{
				res = Mathf.Clamp(s, 0f, totalLength);
			}
			return res;
		}

		private int FindSampleIndex(float s)
		{
			int low = 0;
			int high = cumulativeLengths.Length - 2;

			while (low < high)
			{
				int mid = (low + high + 1) / 2;
				if (cumulativeLengths[mid] <= s)
				{
					low = mid;
				}
				else
				{
					high = mid - 1;
				}
			}

			return low;
		}

		// Nearest curve point to a query, refined between the two neighbouring table samples in shrinking passes
		public float ClosestArcLength(Vector2 query, out Vector2 closestPosition)
		{
			closestPosition = Vector2.zero;
			if (!IsValid) { return 0f; }

			int bestSample = 0;
			float bestSqrDistance = float.MaxValue;
			int sampleCount = cumulativeLengths.Length;

			for (int i = 0; i < sampleCount; i++)
			{
				SampleByArcLength(cumulativeLengths[i], out Vector2 position, out _);
				float sqrDistance = (position - query).sqrMagnitude;
				if (sqrDistance < bestSqrDistance)
				{
					bestSqrDistance = sqrDistance;
					bestSample = i;
				}
			}

			float low = cumulativeLengths[Mathf.Max(0, bestSample - 1)];
			float high = cumulativeLengths[Mathf.Min(sampleCount - 1, bestSample + 1)];
			float bestS = cumulativeLengths[bestSample];

			for (int pass = 0; pass < CLOSEST_REFINE_PASSES; pass++)
			{
				float stepLength = (high - low) / CLOSEST_REFINE_STEPS;
				for (int i = 0; i <= CLOSEST_REFINE_STEPS; i++)
				{
					float s = low + stepLength * i;
					SampleByArcLength(s, out Vector2 position, out _);
					float sqrDistance = (position - query).sqrMagnitude;
					if (sqrDistance < bestSqrDistance)
					{
						bestSqrDistance = sqrDistance;
						bestS = s;
					}
				}

				low = Mathf.Max(0f, bestS - stepLength);
				high = Mathf.Min(totalLength, bestS + stepLength);
			}

			SampleByArcLength(bestS, out closestPosition, out _);
			return bestS;
		}

		public int GetSegmentIndexAtArcLength(float s)
		{
			int res = 0;
			if (IsValid)
			{
				s = WrapOrClampArcLength(s);
				res = Mathf.Min(FindSampleIndex(s) / samplesPerSegment, segmentCount - 1);
			}
			return res;
		}
	}
}
