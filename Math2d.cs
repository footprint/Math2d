using UnityEngine;
using System.Collections;

/*
references:
ref1: http://blog.csdn.net/zaffix/article/details/25077835
ref2: http://www.cnblogs.com/ywxgod/archive/2010/08/06/1793609.html
ref3: https://www.zhihu.com/question/24251545
 */

public class Line2 {
	public Vector2 p1;
	public Vector2 p2;

	float a, b, c;

	Line2(ref Vector2 pt1, ref Vector2 pt2) {
		p1 = pt1;
		p2 = pt2;

		a = p2.y - p1.y;
		b = p1.x - p2.x;
		c = p2.x * p1.y - p1.x * p2.y;

		#if UNITY_EDITOR
		if (Math2d.Approximately(a, 0.0f) && Math2d.Approximately(b, 0.0f)) {
			Debug.LogError("DistanceFromPoint Zero!");
		}
		#endif
	}

	// 计算点与直线的关系。 d = a * x + b * y + c
	// d < 0, 点在直线左侧
	// d = 0，点在直线上
	// d > 0, 点在直线右侧
	public float EvaluatePoint(ref Vector2 pt) {
		return a * pt.x + b * pt.y + c;
	}

	public float DistanceFromPoint(ref Vector2 pt) {
		float eva = EvaluatePoint(ref pt);
		return (eva > 0 ? eva : -eva) / Mathf.Sqrt(a * a + b * b);
	}
}

public class Triangle2 {
	public Vector2 p1;
	public Vector2 p2;
	public Vector2 p3;

	protected Triangle2() {

	}

	public Triangle2(ref Vector2 pt1, ref Vector2 pt2, ref Vector2 pt3) {
		p1 = pt1;
		p2 = pt2;
		p3 = pt3;
	}

	public bool Contains(ref Vector2 pt) {
		return Math2d.IsPointInTriangle(ref pt, ref p1, ref p2, ref p3);
	}

	public bool IntersectsCircle(ref Vector2 center, float radius) {
		return Math2d.IsCircleIntersectTriangle(ref center, radius, ref p1, ref p2, ref p3);
	}

#if UNITY_EDITOR
	public void DebugDraw(Color color, float duration) {
		Debug.DrawLine(p1, p2, color, duration);
		Debug.DrawLine(p2, p3, color, duration);
		Debug.DrawLine(p3, p1, color, duration);
	}
#endif
}

public class Equilateral2 : Triangle2 {
	float edge;
	Vector2 center;

	public float Edge {
		get {
			return edge;
		}
	}

	public Vector2 Center {
		get {
			return center;
		}
	}

	public Equilateral2(Equilateral2 other) {
		p1 = other.p1;
		p2 = other.p2;
		p3 = other.p3;

		edge = other.Edge;
		center = other.Center;
	}

	public Equilateral2(Vector2 center, float edge) {
		this.edge = edge;
		this.center = center;

		float half = edge * 0.5f;
		float d1 = edge * Mathf.Sin(Mathf.PI / 3.0f);
		float d2 = half * Mathf.Tan(Mathf.PI / 6.0f);
	
		p1 = new Vector2(center.x, center.y + d1 - d2); //top
		p2 = new Vector2(center.x - half, center.y - d2); //left
		p3 = new Vector2(center.x + half, center.y - d2); //right
	}

	public void Roate(float degrees) {
		float radian = Mathf.PI * degrees / 180.0f;

		Math2d.RoateVertex(ref p1, ref center, radian);
		Math2d.RoateVertex(ref p2, ref center, radian);
		Math2d.RoateVertex(ref p3, ref center, radian);
	}
}

public class Math2d  {
	static public bool Approximately(float a, float b, float threshold = 0.0001f) {
		return ((a < b)?(b - a):(a - b)) <= threshold;
	}

	//ref2
	static public void RoateVertex(ref Vector2 p, ref Vector2 center, float radian) {
		float x = p.x - center.x, y = p.y - center.y;
		p.x = center.x + Mathf.Cos(radian) * x - Mathf.Sin(radian) * y;
		p.y = center.y + Mathf.Cos(radian) * y + Mathf.Sin(radian) * x;
	}

#if UNITY_EDITOR
	static public void DebugDrawRect(Bounds bound, Vector2 origin, float degrees, Color color, float dt = 0.0f) {
		float hw = bound.size.x * 0.5f;
		float hh = bound.size.y * 0.5f;
	
		Vector2 center = bound.center;
		Vector2 p1 = new Vector2(center.x - hw, center.y + hh);
		Vector2 p2 = new Vector2(center.x + hw, center.y + hh);
		Vector2 p3 = new Vector2(center.x + hw, center.y - hh);
		Vector2 p4 = new Vector2(center.x - hw, center.y - hh);

		if (!Approximately(degrees, 0.0f)) {
			float radian = Mathf.PI * degrees / 180.0f;

			RoateVertex(ref p1, ref origin, radian);
			RoateVertex(ref p2, ref origin, radian);
			RoateVertex(ref p3, ref origin, radian);
			RoateVertex(ref p4, ref origin, radian);
		}
	
		Debug.DrawLine(p1, p2, color, dt);
		Debug.DrawLine(p2, p3, color, dt);
		Debug.DrawLine(p3, p4, color, dt);
		Debug.DrawLine(p4, p1, color, dt);
	}
#endif
	// 判断点P(x, y)与有向直线P1P2的关系. 小于0表示点在直线左侧，等于0表示点在直线上，大于0表示点在直线右侧
	static public float EvaluatePointToLine(ref Vector2 pt, ref Vector2 p1, ref Vector2 p2) {
		float a = p2.y - p1.y;
		float b = p1.x - p2.x;
		float c = p2.x * p1.y - p1.x * p2.y;

		return a * pt.x + b * pt.y + c;
	}

	// 判断点P(x, y)是否在点P1(x1, y1), P2(x2, y2), P3(x3, y3)构成的三角形内（包括边）
	static public bool IsPointInTriangle(ref Vector2 pt, ref Vector2 p1, ref Vector2 p2, ref Vector2 p3) {
		// 分别计算点P与有向直线P1P2, P2P3, P3P1的关系，如果都在同一侧则可判断点在三角形内
		// 注意三角形有可能是顺时针(d>0)，也可能是逆时针(d<0)。
		float d1 = EvaluatePointToLine(ref pt, ref p1, ref p2);
		float d2 = EvaluatePointToLine(ref pt, ref p2, ref p3);

		if ((d1 < 0 && d2 > 0) || (d1 > 0 && d2 < 0)) {
			return false;
		}

		float d3 = EvaluatePointToLine(ref pt, ref p3, ref p1);

		if ((d2 < 0 && d3 > 0) || (d2 > 0 && d3 < 0)) {
			return false;
		}

		return true;
	}

//	static public float DistanceFromPointToLine(ref Vector2 pt, ref Vector2 p1, ref Vector2 p2) {
//		float a = p2.y - p1.y;
//		float b = p1.x - p2.x;
//		float c = p2.x * p1.y - p1.x * p2.y;
//		
//		#if UNITY_EDITOR
//		if (Approximately(a, 0.0f) && Approximately(b, 0.0f)) {
//			Debug.LogError("DistanceFromPoint Zero!");
//			return 0;
//		}
//		#endif
//
//		return Mathf.Abs(a * pt.x + b * pt.y + c) / Mathf.Sqrt(a * a + b * b);
//	}

	// 圆与线段碰撞检测
	// 圆心p(x, y), 半径r, 线段两端点p1(x1, y1)和p2(x2, y2)
	static public bool IsCircleIntersectLineSeg(ref Vector2 center, float radius, ref Vector2 p1, ref Vector2 p2) {
		float vx1 = center.x - p1.x;
		float vy1 = center.y - p1.y;
		float vx2 = p2.x - p1.x;
		float vy2 = p2.y - p1.y;
		
		// len = v2.length()
		float len = Mathf.Sqrt(vx2 * vx2 + vy2 * vy2);
		
		// v2.normalize()
		vx2 /= len;
		vy2 /= len;
		
		// u = v1.dot(v2)
		// u is the vector projection length of vector v1 onto vector v2.
		float u = vx1 * vx2 + vy1 * vy2;
		
		// determine the nearest point on the lineseg
		float x0 = 0.0f;
		float y0 = 0.0f;
		if (u <= 0) {
			// p is on the left of p1, so p1 is the nearest point on lineseg
			x0 = p1.x;
			y0 = p1.y;
		}
		else if (u >= len) {
			// p is on the right of p2, so p2 is the nearest point on lineseg
			x0 = p2.x;
			y0 = p2.y;
		}
		else {
			// p0 = p1 + v2 * u
			// note that v2 is already normalized.
			x0 = p1.x + vx2 * u;
			y0 = p1.y + vy2 * u;
		}

		return (center.x - x0) * (center.x - x0) + (center.y - y0) * (center.y - y0) <= radius * radius;
	}

	// 圆与三角形碰撞检测
	// 圆心(x, y), 半径r，三角形三个顶点(x1, y1),(x2, y2), (x3, y3)
	static public bool IsCircleIntersectTriangle(ref Vector2 center, float radius, ref Vector2 p1, ref Vector2 p2, ref Vector2 p3) {
		// 圆心在三角形内
		if (IsPointInTriangle(ref center, ref p1, ref p2, ref p3)) {
			return true;
		}
		// 圆与三角形任一条边碰撞
		if (IsCircleIntersectLineSeg(ref center, radius, ref p1, ref p2)) {
			return true;
		}
		if (IsCircleIntersectLineSeg(ref center, radius, ref p2, ref p3)) {
			return true;
		}
		if (IsCircleIntersectLineSeg(ref center, radius, ref p3, ref p1)) {
			return true;
		}
		return false;
	}

	static public bool IsCircleIntersectRectangle(Vector2 center, float radius, ref Bounds bound, float degrees = 0.0f) {
		if (!Approximately(degrees, 0)) {
			Vector2 origin = bound.center;
			float radian = Mathf.PI * degrees / 180.0f;
			RoateVertex(ref center, ref origin, radian);
		}

		float hw = bound.size.x * 0.5f;
		float hh = bound.size.y * 0.5f;
		float vx = center.x - bound.center.x;
		float vy = center.y - bound.center.y;
		//abs
		vx = vx > 0 ? vx : -vx;
		vy = vy > 0 ? vy : -vy;

		Vector2 u = new Vector2(vx > hw ? vx - hw : 0, vy > hh ? vy - hh : 0);
		return Vector2.Dot(u, u) <= radius * radius;
	}

	//bug!
//	static public bool IsCircleIntersectRectangle(ref Vector2 center, float radius, ref Bounds bound, float degrees) {
//		float hw = bound.size.x * 0.5f;
//		float hh = bound.size.y * 0.5f;
//		Vector2 rcCenter = bound.center;
//		float radianA = Mathf.PI * degrees / 180.0f;
//		float radianB = Mathf.PI * (90 - degrees) / 180.0f;
//		//center point of top edge
//		float x1 = rcCenter.x + hh * Mathf.Cos(radianA);
//		float y1 = rcCenter.y + hh * Mathf.Sin(radianA);
//		//center point of right edge
//		float x2 = rcCenter.x + hw * Mathf.Cos(radianB);
//		float y2 = rcCenter.y - hw * Mathf.Sin(radianB);
//
//		Vector2 p1 = new Vector2(x1, y1);
//		Vector2 p2 = new Vector2(x2, y2);
//
//		float w1 = Vector2.Distance(rcCenter, p2);
//		float h1 = Vector2.Distance(rcCenter, p1);
//		float w2 = DistanceFromPointToLine(ref center, ref rcCenter, ref p1);
//		float h2 = DistanceFromPointToLine(ref center, ref rcCenter, ref p2);
//		
//		if (w2 > w1 + radius || h2 > h1 + radius) {
//			return false;
//		}
//	
//		if (w2 <= w1 && h2 <= h1) {
//			return true;
//		}
//		
//		return (w2 - w1) * (w2 - w1) + (h2 - h1) * (h2 - h1) <= radius * radius;
//	}
}
