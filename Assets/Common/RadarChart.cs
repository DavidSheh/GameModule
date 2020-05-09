
namespace UnityEngine.UI.Extensions {

	[AddComponentMenu("UI/Extensions/Test")]
	public class RadarChart : MaskableGraphic {
		
		public bool isFill = true;
		[Range(0, 0.99f)] public float fillPercent = 0.8f;
		[Range (0f, 1f)] public float[] values;
		[Range(0f, 360f)] public float angleOffset = 0;
		public bool useStateLine = true;
		public Color lineColor = Color.white;
		public float lineWidth = 0.5f;
		[Range (0f, 1f)] public float lineLength = 0.8f;

		protected override void OnPopulateMesh (VertexHelper vh) {
			Vector2 size = GetComponent <RectTransform> ().rect.size / 2f;
			vh.Clear ();
			int partCount = values.Length;
			for (int i = 0; i < partCount; i++) {
				Vector2 pos1 = GetPoint (size, i) * values[i];
				Vector2 pos2 = isFill ? Vector2.zero : (pos1 * fillPercent);
				Vector2 pos4 = (i + 1 >= partCount) ? (GetPoint (size, 0) * values[0]) : (GetPoint (size, i + 1) * values[i + 1]);
				Vector2 pos3 = isFill ? Vector2.zero : (pos4 * fillPercent);
				vh.AddUIVertexQuad (GetQuad (pos1, pos2, pos3, pos4));
				if (useStateLine) {
					//if (i != 0) {
					//	Vector2 lineEndPos = GetPoint (size, i) * lineLength;
					//	Vector2 lineStartPos = Vector2.zero;
					//	vh.AddUIVertexQuad (GetLine (lineStartPos, lineEndPos));
					//}
					//if (i + 1 == partCount) {
					//	Vector2 lineEndPos = GetPoint (size, 0) * lineLength;
					//	Vector2 lineStartPos = Vector2.zero;
					//	vh.AddUIVertexQuad (GetLine (lineStartPos, lineEndPos));
					//}

                    if (i >= 0 && i< partCount-1)
                    {                   
                        Vector2 lineStartPos = GetPoint(size, i) * lineLength * values[i];
                        Vector2 lineEndPos = GetPoint(size, i+1) * lineLength * values[i+1];
                        vh.AddUIVertexQuad(GetLine(lineStartPos, lineEndPos));
                    }
                    if (i + 1 == partCount)
                    {
                        Vector2 lineStartPos = GetPoint(size, i) * lineLength * values[i];
                        Vector2 lineEndPos = GetPoint(size, 0) * lineLength * values[0];       
                        vh.AddUIVertexQuad(GetLine(lineStartPos, lineEndPos));
                    }
                }
			}
		}

		private UIVertex[] GetLine (Vector2 start, Vector2 end) {
			UIVertex[] vs = new UIVertex[4];
			Vector2[] uv = new Vector2[4];
			uv[0] = new Vector2(0, 0);
			uv[1] = new Vector2(0, 1);
			uv[2] = new Vector2(1, 0);
			uv[3] = new Vector2(1, 1);
			Vector2 v1 = end - start;
			Vector2 v2 = (v1.y == 0f) ? new Vector2 (0f, 1f) : new Vector2 (1f, -v1.x / v1.y);
			v2.Normalize ();
			v2 *= lineWidth / 2f;
			Vector2[] pos = new Vector2[4];
			pos[0] = start + v2;
			pos[1] = end + v2;
			pos[2] = end - v2;
			pos[3] = start - v2;
			for (int i = 0; i < 4; i++) {
				UIVertex v = UIVertex.simpleVert;
				v.color = lineColor;
				v.position = pos[i];
				v.uv0 = uv[i];
				vs[i] = v;
			}
			return vs;
		}

		private Vector2 GetPoint (Vector2 size, int i) {
			int partCount = values.Length;
			float angle = 360f / partCount * i + angleOffset;
			float sin = Mathf.Sin (angle * Mathf.Deg2Rad);
			float cos = Mathf.Cos (angle * Mathf.Deg2Rad);
			return new Vector2 (size.x * cos, size.y * sin);
		}

		private UIVertex[] GetQuad (params Vector2[] vertPos) {
			UIVertex[] vs = new UIVertex[4];
			Vector2[] uv = new Vector2[4];
			uv[0] = new Vector2(0, 0);
			uv[1] = new Vector2(0, 1);
			uv[2] = new Vector2(1, 0);
			uv[3] = new Vector2(1, 1);
			for (int i = 0; i < 4; i++) {
				UIVertex v = UIVertex.simpleVert;
				v.color = color;
				v.position = vertPos[i];
				v.uv0 = uv[i];
				vs[i] = v;
			}
			return vs;
		}
	}
}
