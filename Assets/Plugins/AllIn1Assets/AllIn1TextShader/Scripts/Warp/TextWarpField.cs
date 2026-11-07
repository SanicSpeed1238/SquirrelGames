using System;
using UnityEngine;

namespace AllIn1Assets.AllIn1TextShader
{
	// Photoshop's Warp Text styles as Bezier patches over the box: each style lays out a small control grid in content space (top left origin, y down)
	public class TextWarpField
	{
		// Photoshop's range. Styles whose shape grows linearly with Bend go past it; arcs cannot pass a semicircle, Circle closes at 100 and Fisheye already folds
		public const float MIN_BEND = -100f;
		public const float MAX_BEND = 100f;
		public const float EXTENDED_MAX_BEND = 300f;
		public const float MIN_DISTORTION = -100f;
		public const float MAX_DISTORTION = 100f;

		// The Custom cage: 4x4 points normalized to the box, row major from the top row, x right and y down
		public const int CAGE_SIZE = 4;
		public const int CAGE_POINT_COUNT = CAGE_SIZE * CAGE_SIZE;

		private const int MAX_ORDER = 4;
		private const double ZERO_VALUE = 1e-6;
		private const float MIN_BOX_SIZE = 1e-5f;
		private const float IDENTITY_CAGE_TOLERANCE = 1e-6f;

		// Content y points down, so an arc bowing toward the top sags by a negative amount
		private const double TOWARD_TOP = -1.0;
		private const double TOWARD_BOTTOM = 1.0;

		// Vertical offsets of a row's four control points, in units of the style's displacement
		private static readonly double[] PROFILE_S_CURVE = { 0.0, -1.0, 1.0, 0.0 };
		private static readonly double[] PROFILE_S_CURVE_REVERSED = { 0.0, 1.0, -1.0, 0.0 };
		private static readonly double[] PROFILE_RAMP = { 1.0, 1.0, 0.0, 0.0 };

		private readonly double[] meshX = new double[MAX_ORDER * MAX_ORDER];
		private readonly double[] meshY = new double[MAX_ORDER * MAX_ORDER];
		private readonly double[] scratchX = new double[MAX_ORDER * MAX_ORDER];
		private readonly double[] scratchY = new double[MAX_ORDER * MAX_ORDER];
		private readonly double[] cageX = new double[MAX_ORDER * MAX_ORDER];
		private readonly double[] cageY = new double[MAX_ORDER * MAX_ORDER];
		private int uOrder;
		private int vOrder;
		private int count;

		private readonly double[] weightsU = new double[MAX_ORDER];
		private readonly double[] derivativesU = new double[MAX_ORDER];
		private readonly double[] weightsV = new double[MAX_ORDER];
		private readonly double[] derivativesV = new double[MAX_ORDER];

		private Rect box;
		private bool isCircle;
		private double circleRadius;
		private double circleSign;
		private bool isBanner;
		private bool bannerVertical;
		private double bannerAmplitude;
		private double bannerWaves;
		private double bannerPhase;
		private TextWarpPin bannerPin;
		private bool identity = true;

		public bool IsIdentity { get { return identity; } }
		public Rect Box { get { return box; } }

		// waves, phase (in cycles) and pin only shape a Banner; customCage only a Custom warp
		public void Configure(TextWarpStyle style, float bend, float horizontalDistortion, float verticalDistortion, TextWarpOrientation orientation, Rect box, Vector2[] customCage = null, float waves = 1f, float phase = 0f, TextWarpPin pin = TextWarpPin.NONE)
		{
			this.box = box;
			identity = true;
			isCircle = style == TextWarpStyle.CIRCLE;
			isBanner = style == TextWarpStyle.BANNER;

			if (box.width < MIN_BOX_SIZE || box.height < MIN_BOX_SIZE) { return; }

			if (isBanner)
			{
				// Bend 100 lifts the crests by the text's full thickness across the wave
				bannerVertical = orientation == TextWarpOrientation.VERTICAL;
				double across = bannerVertical ? box.width : box.height;
				bannerAmplitude = Mathf.Clamp(bend, -EXTENDED_MAX_BEND, EXTENDED_MAX_BEND) / 100.0 * across;
				bannerWaves = waves;
				bannerPhase = phase;
				bannerPin = pin;
				identity = Math.Abs(bannerAmplitude) < ZERO_VALUE;
				return;
			}

			if (style == TextWarpStyle.CUSTOM)
			{
				if (customCage == null || customCage.Length < CAGE_POINT_COUNT || IsIdentityCage(customCage)) { return; }

				ResetGrid(CAGE_SIZE, CAGE_SIZE);
				for (int i = 0; i < CAGE_POINT_COUNT; i++)
				{
					meshX[i] = customCage[i].x * (double)box.width;
					meshY[i] = customCage[i].y * (double)box.height;
				}
				identity = false;
				return;
			}

			float maxBend = GetMaxBend(style);
			double clampedBend = Mathf.Clamp(bend, -maxBend, maxBend);
			double horizontal = Mathf.Clamp(horizontalDistortion, MIN_DISTORTION, MAX_DISTORTION);
			double vertical = Mathf.Clamp(verticalDistortion, MIN_DISTORTION, MAX_DISTORTION);

			if (isCircle)
			{
				if (Math.Abs(clampedBend) < ZERO_VALUE) { return; }

				// Bend 100 wraps the box's middle line once around the circle
				double span = Math.Abs(clampedBend) / 100.0 * 2.0 * Math.PI;
				circleRadius = box.width / span;
				circleSign = clampedBend > 0.0 ? 1.0 : -1.0;
				identity = false;
				return;
			}

			if (Math.Abs(clampedBend) < ZERO_VALUE && Math.Abs(horizontal) < ZERO_VALUE && Math.Abs(vertical) < ZERO_VALUE) { return; }

			// Vertical builds the same grid along the height of the swapped box and turns it back; Twist looks the same both ways
			if (orientation == TextWarpOrientation.VERTICAL && style != TextWarpStyle.TWIST)
			{
				BuildAlongWidth(style, clampedBend, box.height, box.width);
				Transpose();
			}
			else
			{
				BuildAlongWidth(style, clampedBend, box.width, box.height);
			}

			Distort(horizontal, vertical);
			identity = false;
		}

		// Also returns how the output moves per unit of local x and of local y (the Jacobian's columns; outside the box, those of the nearest edge point)
		public Vector2 Evaluate(Vector2 position, out Vector2 derivativeX, out Vector2 derivativeY)
		{
			if (identity)
			{
				derivativeX = Vector2.right;
				derivativeY = Vector2.up;
				return position;
			}

			if (isCircle)
			{
				return EvaluateCircle(position, out derivativeX, out derivativeY);
			}

			if (isBanner)
			{
				return EvaluateBanner(position, out derivativeX, out derivativeY);
			}

			double width = box.width;
			double height = box.height;
			double u = (position.x - box.xMin) / width;
			double v = (box.yMax - position.y) / height;
			double clampedU = Math.Min(1.0, Math.Max(0.0, u));
			double clampedV = Math.Min(1.0, Math.Max(0.0, v));

			EvaluatePatch(clampedU, clampedV, out double x, out double y, out double dxdu, out double dydu, out double dxdv, out double dydv);

			// Content space runs y down, so v and the content y both flip against local y
			derivativeX = new Vector2((float)(dxdu / width), (float)(-dydu / width));
			derivativeY = new Vector2((float)(-dxdv / height), (float)(dydv / height));

			// Outside the box the patch continues along its edge tangents, so letters pushed out by effects keep moving smoothly
			Vector2 res = new Vector2(box.xMin + (float)x, box.yMax - (float)y);
			if (u != clampedU || v != clampedV)
			{
				Vector2 onEdge = new Vector2(box.xMin + (float)(clampedU * width), box.yMax - (float)(clampedV * height));
				Vector2 beyond = position - onEdge;
				res += derivativeX * beyond.x + derivativeY * beyond.y;
			}

			return res;
		}

		// Position only: inside the box it skips the derivatives, which the per vertex path never uses
		public Vector2 Evaluate(Vector2 position)
		{
			if (identity || isCircle || isBanner) { return Evaluate(position, out _, out _); }

			double u = (position.x - box.xMin) / (double)box.width;
			double v = (box.yMax - position.y) / (double)box.height;
			if (u < 0.0 || u > 1.0 || v < 0.0 || v > 1.0) { return Evaluate(position, out _, out _); }

			Bernstein(uOrder, u, weightsU, derivativesU);
			Bernstein(vOrder, v, weightsV, derivativesV);

			double x = 0.0;
			double y = 0.0;
			for (int row = 0; row < vOrder; row++)
			{
				double weightV = weightsV[row];
				int first = row * uOrder;
				for (int column = 0; column < uOrder; column++)
				{
					double weight = weightsU[column] * weightV;
					x += weight * meshX[first + column];
					y += weight * meshY[first + column];
				}
			}

			return new Vector2(box.xMin + (float)x, box.yMax - (float)y);
		}

		// The configured warp as a 4x4 cage (exact degree elevation, the surface is unchanged). Circle and Banner are not patches and have none
		public bool TryGetCage(Vector2[] cage)
		{
			if (cage == null || cage.Length < CAGE_POINT_COUNT || isCircle || isBanner) { return false; }

			if (identity)
			{
				SetIdentityCage(cage);
				return true;
			}

			int columns = uOrder;
			int rows = vOrder;
			Array.Copy(meshX, cageX, count);
			Array.Copy(meshY, cageY, count);
			while (columns < CAGE_SIZE) { ElevateColumns(ref columns, rows); }
			while (rows < CAGE_SIZE) { ElevateRows(columns, ref rows); }

			for (int i = 0; i < CAGE_POINT_COUNT; i++)
			{
				cage[i] = new Vector2((float)(cageX[i] / box.width), (float)(cageY[i] / box.height));
			}
			return true;
		}

		public static float GetMaxBend(TextWarpStyle style)
		{
			float res = MAX_BEND;
			switch (style)
			{
				case TextWarpStyle.FLAG:
				case TextWarpStyle.BANNER:
				case TextWarpStyle.WAVE:
				case TextWarpStyle.FISH:
				case TextWarpStyle.RISE:
				case TextWarpStyle.TWIST:
				case TextWarpStyle.INFLATE:
				case TextWarpStyle.SQUEEZE:
					res = EXTENDED_MAX_BEND;
					break;
			}
			return res;
		}

		public static Vector2[] CreateIdentityCage()
		{
			Vector2[] res = new Vector2[CAGE_POINT_COUNT];
			SetIdentityCage(res);
			return res;
		}

		public static void SetIdentityCage(Vector2[] cage)
		{
			for (int i = 0; i < CAGE_POINT_COUNT; i++)
			{
				cage[i] = GetIdentityCagePoint(i);
			}
		}

		public static Vector2 GetIdentityCagePoint(int index)
		{
			return new Vector2((index % CAGE_SIZE) / (float)(CAGE_SIZE - 1), (index / CAGE_SIZE) / (float)(CAGE_SIZE - 1));
		}

		public static bool IsIdentityCage(Vector2[] cage)
		{
			for (int i = 0; i < CAGE_POINT_COUNT; i++)
			{
				if ((cage[i] - GetIdentityCagePoint(i)).sqrMagnitude > IDENTITY_CAGE_TOLERANCE * IDENTITY_CAGE_TOLERANCE) { return false; }
			}
			return true;
		}

		public static Vector2 CagePointToLocal(Vector2 cagePoint, Rect box)
		{
			return new Vector2(box.xMin + cagePoint.x * box.width, box.yMax - cagePoint.y * box.height);
		}

		public static Vector2 LocalToCagePoint(Vector2 localPosition, Rect box)
		{
			return new Vector2((localPosition.x - box.xMin) / box.width, (box.yMax - localPosition.y) / box.height);
		}

		// One more control point per row: point i becomes i/n of point i - 1 plus the rest of point i
		private void ElevateColumns(ref int columns, int rows)
		{
			Array.Copy(cageX, scratchX, columns * rows);
			Array.Copy(cageY, scratchY, columns * rows);

			int newColumns = columns + 1;
			for (int row = 0; row < rows; row++)
			{
				for (int i = 0; i < newColumns; i++)
				{
					int to = row * newColumns + i;
					int first = row * columns;
					if (i == 0 || i == columns)
					{
						int from = first + (i == 0 ? 0 : columns - 1);
						cageX[to] = scratchX[from];
						cageY[to] = scratchY[from];
					}
					else
					{
						double alpha = (double)i / columns;
						cageX[to] = alpha * scratchX[first + i - 1] + (1.0 - alpha) * scratchX[first + i];
						cageY[to] = alpha * scratchY[first + i - 1] + (1.0 - alpha) * scratchY[first + i];
					}
				}
			}
			columns = newColumns;
		}

		private void ElevateRows(int columns, ref int rows)
		{
			Array.Copy(cageX, scratchX, columns * rows);
			Array.Copy(cageY, scratchY, columns * rows);

			int newRows = rows + 1;
			for (int column = 0; column < columns; column++)
			{
				for (int j = 0; j < newRows; j++)
				{
					int to = j * columns + column;
					if (j == 0 || j == rows)
					{
						int from = (j == 0 ? 0 : rows - 1) * columns + column;
						cageX[to] = scratchX[from];
						cageY[to] = scratchY[from];
					}
					else
					{
						double alpha = (double)j / rows;
						cageX[to] = alpha * scratchX[(j - 1) * columns + column] + (1.0 - alpha) * scratchX[j * columns + column];
						cageY[to] = alpha * scratchY[(j - 1) * columns + column] + (1.0 - alpha) * scratchY[j * columns + column];
					}
				}
			}
			rows = newRows;
		}

		// The box's middle line becomes an arc of the circle with its length kept. Positive bend puts the text on top of the circle, negative underneath
		private Vector2 EvaluateCircle(Vector2 position, out Vector2 derivativeX, out Vector2 derivativeY)
		{
			double centerX = box.center.x;
			double centerY = box.center.y;
			double dx = position.x - centerX;
			double dy = position.y - centerY;

			double theta = dx / circleRadius;
			double sinTheta = Math.Sin(theta);
			double cosTheta = Math.Cos(theta);
			double sinHalf = Math.Sin(theta * 0.5);
			double radius = circleRadius + circleSign * dy;

			// A short, tall text bent hard reaches past the center: it pinches there instead of crossing over
			if (radius <= 0.0)
			{
				derivativeX = Vector2.zero;
				derivativeY = Vector2.zero;
				return new Vector2((float)centerX, (float)(centerY - circleSign * circleRadius));
			}

			// 1 - cos written as 2 sin^2 so a huge radius (tiny bend) does not cancel out
			double x = centerX + radius * sinTheta;
			double y = centerY + dy * cosTheta - circleSign * 2.0 * circleRadius * sinHalf * sinHalf;

			derivativeX = new Vector2((float)(radius * cosTheta / circleRadius), (float)(-circleSign * sinTheta * radius / circleRadius));
			derivativeY = new Vector2((float)(circleSign * sinTheta), (float)cosTheta);

			return new Vector2((float)x, (float)y);
		}

		// A sine wave running along the box that the text rides; pinned, it grows from the held end like a flag from its pole. Defined everywhere, so no edge extension
		private Vector2 EvaluateBanner(Vector2 position, out Vector2 derivativeX, out Vector2 derivativeY)
		{
			// Along the wave: x from the left, or y from the top when vertical; the text moves across it
			double length = bannerVertical ? box.height : box.width;
			double along = bannerVertical ? (box.yMax - position.y) / length : (position.x - box.xMin) / length;

			double angle = 2.0 * Math.PI * (bannerWaves * along - bannerPhase);
			double sin = Math.Sin(angle);
			double cos = Math.Cos(angle);

			double pin = 1.0;
			double pinSlope = 0.0;
			if (bannerPin == TextWarpPin.START)
			{
				pin = Math.Min(1.0, Math.Max(0.0, along));
				pinSlope = along > 0.0 && along < 1.0 ? 1.0 : 0.0;
			}
			else if (bannerPin == TextWarpPin.END)
			{
				pin = Math.Min(1.0, Math.Max(0.0, 1.0 - along));
				pinSlope = along > 0.0 && along < 1.0 ? -1.0 : 0.0;
			}

			double offset = bannerAmplitude * pin * sin;
			double offsetSlope = bannerAmplitude * (pinSlope * sin + pin * cos * 2.0 * Math.PI * bannerWaves) / length;

			if (bannerVertical)
			{
				// Along runs down the text, so the offset changes against local y
				derivativeX = Vector2.right;
				derivativeY = new Vector2((float)-offsetSlope, 1f);
				return new Vector2(position.x + (float)offset, position.y);
			}

			derivativeX = new Vector2(1f, (float)offsetSlope);
			derivativeY = Vector2.up;
			return new Vector2(position.x, position.y + (float)offset);
		}

		private void EvaluatePatch(double u, double v, out double x, out double y, out double dxdu, out double dydu, out double dxdv, out double dydv)
		{
			Bernstein(uOrder, u, weightsU, derivativesU);
			Bernstein(vOrder, v, weightsV, derivativesV);

			x = 0.0; y = 0.0; dxdu = 0.0; dydu = 0.0; dxdv = 0.0; dydv = 0.0;
			for (int row = 0; row < vOrder; row++)
			{
				for (int column = 0; column < uOrder; column++)
				{
					int index = row * uOrder + column;
					double weight = weightsU[column] * weightsV[row];
					double weightDu = derivativesU[column] * weightsV[row];
					double weightDv = weightsU[column] * derivativesV[row];

					x += weight * meshX[index];
					y += weight * meshY[index];
					dxdu += weightDu * meshX[index];
					dydu += weightDu * meshY[index];
					dxdv += weightDv * meshX[index];
					dydv += weightDv * meshY[index];
				}
			}
		}

		// Bernstein basis of degree (order - 1) at t and its derivative
		private static void Bernstein(int order, double t, double[] weights, double[] derivatives)
		{
			double s = 1.0 - t;
			switch (order)
			{
				case 2:
					weights[0] = s; weights[1] = t;
					derivatives[0] = -1.0; derivatives[1] = 1.0;
					break;
				case 3:
					weights[0] = s * s; weights[1] = 2.0 * s * t; weights[2] = t * t;
					derivatives[0] = -2.0 * s; derivatives[1] = 2.0 * (s - t); derivatives[2] = 2.0 * t;
					break;
				default:
					weights[0] = s * s * s; weights[1] = 3.0 * s * s * t; weights[2] = 3.0 * s * t * t; weights[3] = t * t * t;
					derivatives[0] = -3.0 * s * s; derivatives[1] = 3.0 * s * (s - 2.0 * t); derivatives[2] = 3.0 * t * (2.0 * s - t); derivatives[3] = 3.0 * t * t;
					break;
			}
		}

		// Each style's control grid laid out along the width of a width by height box, in content space
		private void BuildAlongWidth(TextWarpStyle style, double bend, double width, double height)
		{
			double halfAngle = Math.Abs(bend) * Math.PI / 200.0;
			double displacement = 2.0 * height * bend / 100.0;
			bool flat = Math.Abs(bend) < ZERO_VALUE;

			switch (style)
			{
				case TextWarpStyle.ARC:
					if (flat) { FlatGrid(4, 2, width, height); break; }
					// Both edges arc through the same angle, and the top one also swings about the bottom corners, so the band keeps the text's height
					ResetGrid(4, 2);
					double swing = height * Math.Sin(halfAngle);
					ArcRow(0, -swing, width + swing, height * (1.0 - Math.Cos(halfAngle)), halfAngle, TOWARD_TOP);
					ArcRow(1, 0.0, width, height, halfAngle, TOWARD_TOP);
					if (bend < 0.0) { FlipUpsideDown(height); }
					break;

				case TextWarpStyle.ARC_LOWER:
				case TextWarpStyle.ARC_UPPER:
					if (flat) { FlatGrid(4, 2, width, height); break; }
					ResetGrid(4, 2);
					FlatRow(0, width, 0.0);
					ArcRow(1, 0.0, width, height, halfAngle, bend > 0.0 ? TOWARD_BOTTOM : TOWARD_TOP);
					if (style == TextWarpStyle.ARC_UPPER) { FlipUpsideDown(height); }
					break;

				case TextWarpStyle.ARCH:
				case TextWarpStyle.BULGE:
					if (flat) { FlatGrid(4, 2, width, height); break; }
					double topSag = bend > 0.0 ? TOWARD_TOP : TOWARD_BOTTOM;
					ResetGrid(4, 2);
					ArcRow(0, 0.0, width, 0.0, halfAngle, topSag);
					ArcRow(1, 0.0, width, height, halfAngle, style == TextWarpStyle.BULGE ? -topSag : topSag);
					break;

				case TextWarpStyle.SHELL_LOWER:
				case TextWarpStyle.SHELL_UPPER:
					if (flat) { FlatGrid(4, 4, width, height); break; }
					ShellGrid(bend, halfAngle, width, height);
					if (style == TextWarpStyle.SHELL_UPPER) { FlipUpsideDown(height); }
					break;

				case TextWarpStyle.FLAG:
					FlatGrid(4, 2, width, height);
					OffsetRow(0, PROFILE_S_CURVE, displacement);
					OffsetRow(1, PROFILE_S_CURVE, displacement);
					break;

				case TextWarpStyle.FISH:
					FlatGrid(4, 2, width, height);
					OffsetRow(0, PROFILE_S_CURVE, displacement);
					OffsetRow(1, PROFILE_S_CURVE_REVERSED, displacement);
					break;

				case TextWarpStyle.RISE:
					FlatGrid(4, 2, width, height);
					OffsetRow(0, PROFILE_RAMP, displacement);
					OffsetRow(1, PROFILE_RAMP, displacement);
					break;

				case TextWarpStyle.WAVE:
					// Top and bottom stay straight and only the middle row waves
					FlatGrid(4, 3, width, height);
					OffsetRow(1, PROFILE_S_CURVE_REVERSED, displacement);
					break;

				case TextWarpStyle.FISHEYE:
					// The inner points slide toward their nearest corners, reaching them at bend 50
					FlatGrid(4, 4, width, height);
					double pull = bend / 50.0;
					for (int row = 1; row <= 2; row++)
					{
						for (int column = 1; column <= 2; column++)
						{
							int index = At(column, row);
							meshX[index] += pull * ((column == 1 ? 0.0 : width) - meshX[index]);
							meshY[index] += pull * ((row == 1 ? 0.0 : height) - meshY[index]);
						}
					}
					break;

				case TextWarpStyle.INFLATE:
				case TextWarpStyle.SQUEEZE:
					// A quadratic grid whose edge middles move: all outward for Inflate, the sides inward for Squeeze
					FlatGrid(3, 3, width, height);
					double sideShift = (style == TextWarpStyle.INFLATE ? 1.0 : -1.0) * width * bend / 200.0;
					double edgeShift = height * bend / 200.0;
					meshY[At(1, 0)] -= edgeShift;
					meshY[At(1, 2)] += edgeShift;
					meshX[At(0, 1)] -= sideShift;
					meshX[At(2, 1)] += sideShift;
					break;

				case TextWarpStyle.TWIST:
					// Each inner point slides one step around the inner ring, clockwise for positive bend
					FlatGrid(4, 4, width, height);
					double stepX = width * Math.Abs(bend) / 100.0;
					double stepY = height * Math.Abs(bend) / 100.0;
					if (bend > 0.0)
					{
						meshX[At(1, 1)] += stepX;
						meshY[At(2, 1)] += stepY;
						meshX[At(2, 2)] -= stepX;
						meshY[At(1, 2)] -= stepY;
					}
					else if (bend < 0.0)
					{
						meshY[At(1, 1)] += stepY;
						meshX[At(2, 1)] -= stepX;
						meshY[At(2, 2)] -= stepY;
						meshX[At(1, 2)] += stepX;
					}
					break;

				default:
					FlatGrid(4, 2, width, height);
					break;
			}
		}

		// Two straight rows, a third whose ends swing about the box's corners, and an edge that fans out (positive bend) or arcs into the box (negative)
		private void ShellGrid(double bend, double halfAngle, double width, double height)
		{
			bool fanOut = bend > 0.0;
			double sin = Math.Sin(halfAngle);
			double cos = Math.Cos(halfAngle);

			ResetGrid(4, 4);
			FlatRow(0, width, 0.0);
			FlatRow(1, width, height / 3.0);

			// The swinging ends hang from the top corners when fanning out and stand on the bottom ones otherwise
			double radius = fanOut ? 2.0 * height / 3.0 : height / 3.0;
			double endY = fanOut ? radius * cos : height - radius * cos;
			Put(0, 2, -radius * sin, endY);
			Put(1, 2, width / 3.0, 2.0 * height / 3.0);
			Put(2, 2, 2.0 * width / 3.0, 2.0 * height / 3.0);
			Put(3, 2, width + radius * sin, endY);

			if (fanOut)
			{
				ArcRow(3, -height * sin, width + height * sin, height * cos, halfAngle, TOWARD_BOTTOM);
			}
			else
			{
				ArcRow(3, 0.0, width, height, halfAngle, TOWARD_TOP);
			}
		}

		// Photoshop's Distortion: every row, then every column, scales about the middle of its two end points, from 1 - k on the first line to 1 + k on the last
		private void Distort(double horizontal, double vertical)
		{
			if (Math.Abs(vertical) >= ZERO_VALUE && vOrder > 1)
			{
				for (int row = 0; row < vOrder; row++)
				{
					ScaleLine(At(0, row), 1, uOrder, 1.0 + (2.0 * row / (vOrder - 1) - 1.0) * vertical / 100.0);
				}
			}

			if (Math.Abs(horizontal) >= ZERO_VALUE && uOrder > 1)
			{
				for (int column = 0; column < uOrder; column++)
				{
					ScaleLine(At(column, 0), uOrder, vOrder, 1.0 + (2.0 * column / (uOrder - 1) - 1.0) * horizontal / 100.0);
				}
			}
		}

		// The grid points first, first + stride... scaled about the midpoint of that line's two end points
		private void ScaleLine(int first, int stride, int pointCount, double scale)
		{
			int last = first + stride * (pointCount - 1);
			double middleX = 0.5 * (meshX[first] + meshX[last]);
			double middleY = 0.5 * (meshY[first] + meshY[last]);
			for (int k = 0; k < pointCount; k++)
			{
				int index = first + stride * k;
				meshX[index] = middleX + (meshX[index] - middleX) * scale;
				meshY[index] = middleY + (meshY[index] - middleY) * scale;
			}
		}

		private void ResetGrid(int columns, int rows)
		{
			uOrder = columns;
			vOrder = rows;
			count = columns * rows;
		}

		private int At(int column, int row)
		{
			return row * uOrder + column;
		}

		private void Put(int column, int row, double x, double y)
		{
			int index = At(column, row);
			meshX[index] = x;
			meshY[index] = y;
		}

		// Control points spread evenly over the box: the identity patch
		private void FlatGrid(int columns, int rows, double width, double height)
		{
			ResetGrid(columns, rows);
			for (int row = 0; row < rows; row++)
			{
				for (int column = 0; column < columns; column++)
				{
					Put(column, row, width * column / (columns - 1), height * row / (rows - 1));
				}
			}
		}

		private void FlatRow(int row, double width, double y)
		{
			for (int column = 0; column < uOrder; column++)
			{
				Put(column, row, width * column / (uOrder - 1), y);
			}
		}

		// A row bowed into a circular arc over the chord (startX, y) to (endX, y), leaving it at halfAngle, with the standard cubic handles of length 4/3 r tan(halfAngle / 2)
		private void ArcRow(int row, double startX, double endX, double y, double halfAngle, double sag)
		{
			double radius = (endX - startX) / (2.0 * Math.Sin(halfAngle));
			double handle = 4.0 / 3.0 * radius * Math.Tan(halfAngle * 0.5);
			double handleX = handle * Math.Cos(halfAngle);
			double handleY = sag * handle * Math.Sin(halfAngle);

			Put(0, row, startX, y);
			Put(1, row, startX + handleX, y + handleY);
			Put(2, row, endX - handleX, y + handleY);
			Put(3, row, endX, y);
		}

		private void OffsetRow(int row, double[] profile, double displacement)
		{
			for (int column = 0; column < uOrder; column++)
			{
				meshY[At(column, row)] += profile[column] * displacement;
			}
		}

		// Rows swap top for bottom and every y becomes height - y
		private void FlipUpsideDown(double height)
		{
			for (int top = 0, bottom = vOrder - 1; top <= bottom; top++, bottom--)
			{
				for (int column = 0; column < uOrder; column++)
				{
					int upper = At(column, top);
					int lower = At(column, bottom);
					double upperX = meshX[upper];
					double upperY = meshY[upper];
					meshX[upper] = meshX[lower];
					meshY[upper] = height - meshY[lower];
					meshX[lower] = upperX;
					meshY[lower] = height - upperY;
				}
			}
		}

		// Rows become columns and x becomes y: a construction made along the width becomes one along the height
		private void Transpose()
		{
			Array.Copy(meshX, scratchX, count);
			Array.Copy(meshY, scratchY, count);

			int columns = uOrder;
			int rows = vOrder;
			ResetGrid(rows, columns);
			for (int row = 0; row < rows; row++)
			{
				for (int column = 0; column < columns; column++)
				{
					int from = row * columns + column;
					Put(row, column, scratchY[from], scratchX[from]);
				}
			}
		}
	}
}
