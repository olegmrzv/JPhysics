namespace JPhysics.Unity
{
    using LinearMath;
    using UnityEngine;

    static class JExtendens
    {
        public static JMatrix ConvertToJMatrix(this Quaternion q)
        {
            var w = q.w;
            var x = q.x;
            var y = q.y;
            var z = q.z;
            return new JMatrix
            {
                M11 = 1 - 2 * (y * y) - 2 * (z * z),
                M12 = 2 * x * y + 2 * z * w,
                M13 = 2 * x * z - 2 * y * w,

                M21 = 2 * x * y - 2 * z * w,
                M22 = 1 - 2 * (x * x) - 2 * (z * z),
                M23 = 2 * y * z + 2 * x * w,

                M31 = 2 * x * z + 2 * y * w,
                M32 = 2 * y * z - 2 * x * w,
                M33 = 1 - 2 * (x * x) - 2 * (y * y)
            };
        }

        public static Quaternion ConvertToQuaternion(this JMatrix m)
        {
            var q = new Quaternion
            {
                w = Mathf.Sqrt(Mathf.Max(0, 1 + m.M11 + m.M22 + m.M33)) / 2,
                x = Mathf.Sqrt(Mathf.Max(0, 1 + m.M11 - m.M22 - m.M33)) / 2,
                y = Mathf.Sqrt(Mathf.Max(0, 1 - m.M11 + m.M22 - m.M33)) / 2,
                z = Mathf.Sqrt(Mathf.Max(0, 1 - m.M11 - m.M22 + m.M33)) / 2
            };
            q.x *= Mathf.Sign(q.x * (m.M23 - m.M32));
            q.y *= Mathf.Sign(q.y * (m.M31 - m.M13));
            q.z *= Mathf.Sign(q.z * (m.M12 - m.M21));
            return q;
        }

        public static Quaternion ConvertToQuaternion(this JMatrix m, Quaternion q)
        {
            q.w = Mathf.Sqrt(Mathf.Max(0, 1 + m.M11 + m.M22 + m.M33)) / 2;
            q.x = Mathf.Sqrt(Mathf.Max(0, 1 + m.M11 - m.M22 - m.M33)) / 2;
            q.y = Mathf.Sqrt(Mathf.Max(0, 1 - m.M11 + m.M22 - m.M33)) / 2;
            q.z = Mathf.Sqrt(Mathf.Max(0, 1 - m.M11 - m.M22 + m.M33)) / 2;
            q.x *= Mathf.Sign(q.x * (m.M23 - m.M32));
            q.y *= Mathf.Sign(q.y * (m.M31 - m.M13));
            q.z *= Mathf.Sign(q.z * (m.M12 - m.M21));
            return q;
        }

        public static JVector ConvertToJVector(this Vector3 v)
        {
            return new JVector(v.x, v.y, v.z);
        }

        public static Vector3 ConvertToVector3(this JVector v)
        {
            return new Vector3(v.X, v.Y, v.Z);
        }

        public static Vector3 ConvertToVector3(this JVector v, Vector3 v3)
        {
            v3.x = v.X;
            v3.y = v.Y;
            v3.z = v.Z;
            return v3;
        }

        public static Quaternion ConvertToQuaternion(this JQuaternion jq, Quaternion q)
        {
            q.w = jq.W;
            q.x = jq.X;
            q.y = jq.Y;
            q.z = jq.Z;
            return q;
        }
    }
}
