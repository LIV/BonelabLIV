using UnityEngine;
using System.Runtime.InteropServices;
using System;

namespace LIV.SDK.Unity
{
    public struct SDKConstants
    {
        public const string SDKID = "L9IBZ59GZQXBLQ6IYTBV8AXX4NGDYCQ0";
        public const string SDKVersion = "1.5.4";
        public const string EngineName = "unity";
    }

    public enum Priority : sbyte
    {
        None = 0,
        Game = 63
    }

    [Flags]
    public enum Features : ulong
    {
        None = 0L,
        BackgroundRender = 1L,
        ForegroundRender = 1L << 1,
        ComplexClipPlane = 1L << 2,
        BackgroundDepthRender = 1L << 3,
        OverridePostProcessing = 1L << 4,
        FixForegroundAlpha = 1L << 5,
        GroundClipPlane = 1L << 6,
        ReleaseControl = 1L << 15,
        OptimizedRender = 1L << 28,
        InterlacedRender = 1L << 29,
        DebugClipPlane = 1L << 48,
    }

    public enum TextureID : uint
    {
        Undefined = 0,
        BackgroundColorBufferID = 10,
        ForegroundColorBufferID = 20,
        OptimizedColorBufferID = 30
    }

    public enum TextureType : uint
    {
        Undefined = 0,
        ColorBuffer = 1
    }

    public enum TextureFormat : uint
    {
        Undefined = 0,
        Argb32 = 10
    }

    public enum TextureDevice : uint
    {
        Undefined = 0,
        Raw = 1,
        Directx = 2,
        Opengl = 3,
        Vulkan = 4,
        Metal = 5
    }

    public enum TextureColorSpace : uint
    {
        Undefined = 0,
        Linear = 1,
        Srgb = 2,
    }

    public enum RenderingPipeline : uint
    {
        Undefined = 0,
        Forward = 1,
        Deferred = 2,
        VertexLit = 3,
        Universal = 4,
        HighDefinition = 5
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDKResolution
    {
        public int width, height;
        public static SDKResolution Zero {
            get {
                return new SDKResolution() { width = 0, height = 0 };
            }
        }

        public override string ToString()
        {
            return 
$@"SDKResolution:
width: {width}
height: {height}";
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDKVector3
    {
        public float x, y, z;
        public static SDKVector3 Zero {
            get {
                return new SDKVector3() { x = 0, y = 0, z = 0 };
            }
        }

        public static SDKVector3 One {
            get {
                return new SDKVector3() { x = 1, y = 1, z = 1 };
            }
        }

        public static SDKVector3 Forward {
            get {
                return new SDKVector3() { x = 0, y = 0, z = 1 };
            }
        }

        public static SDKVector3 Up {
            get {
                return new SDKVector3() { x = 0, y = 1, z = 0 };
            }
        }

        public static SDKVector3 Right {
            get {
                return new SDKVector3() { x = 1, y = 0, z = 0 };
            }
        }

        public static implicit operator Vector3(SDKVector3 v)
        {
            return new Vector3(v.x, v.y, v.z);
        }

        public static implicit operator SDKVector3(Vector3 v)
        {
            return new SDKVector3() { x = v.x, y = v.y, z = v.z };
        }

        // Delete begin
        public static SDKVector3 operator +(SDKVector3 lhs, SDKVector3 rhs)
        {
            SDKVector3 res;
            res.x = lhs.x + rhs.x;
            res.y = lhs.y + rhs.y;
            res.z = lhs.z + rhs.z;
            return res;
        }

        public static SDKVector3 operator -(SDKVector3 lhs, SDKVector3 rhs)
        {
            SDKVector3 res;
            res.x = lhs.x - rhs.x;
            res.y = lhs.y - rhs.y;
            res.z = lhs.z - rhs.z;
            return res;
        }

        public static SDKVector3 operator *(SDKVector3 lhs, SDKVector3 rhs)
        {
            SDKVector3 res;
            res.x = lhs.x * rhs.x;
            res.y = lhs.y * rhs.y;
            res.z = lhs.z * rhs.z;
            return res;
        }

        public static SDKVector3 operator *(SDKVector3 lhs, float rhs)
        {
            SDKVector3 res;
            res.x = lhs.x * rhs;
            res.y = lhs.y * rhs;
            res.z = lhs.z * rhs;
            return res;
        }
        // delete end

        public override string ToString()
        {
            return
$@"SDKVector3:
x: {x}
y: {y}
z: {z}";
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDKQuaternion
    {
        public float x, y, z, w;
        public static SDKQuaternion Identity {
            get {
                return new SDKQuaternion() { x = 0, y = 0, z = 0, w = 1.0f };
            }
        }

        public static implicit operator Quaternion(SDKQuaternion v)
        {
            return new Quaternion(v.x, v.y, v.z, v.w);
        }

        public static implicit operator SDKQuaternion(Quaternion v)
        {
            return new SDKQuaternion() { x = v.x, y = v.y, z = v.z, w = v.w };
        }

        // Delete begin
        public static SDKQuaternion Euler(float pitch, float yaw, float roll)
        {
            float rollOver2 = roll * 0.5f;
            float sinRollOver2 = Mathf.Sin(rollOver2);
            float cosRollOver2 = Mathf.Cos(rollOver2);
            float pitchOver2 = pitch * 0.5f;
            float sinPitchOver2 = Mathf.Sin(pitchOver2);
            float cosPitchOver2 = Mathf.Cos(pitchOver2);
            float yawOver2 = yaw * 0.5f;
            float sinYawOver2 = Mathf.Sin(yawOver2);
            float cosYawOver2 = Mathf.Cos(yawOver2);

            var w = cosYawOver2 * cosPitchOver2 * cosRollOver2 + sinYawOver2 * sinPitchOver2 * sinRollOver2;
            var x = cosYawOver2 * sinPitchOver2 * cosRollOver2 + sinYawOver2 * cosPitchOver2 * sinRollOver2;
            var y = sinYawOver2 * cosPitchOver2 * cosRollOver2 - cosYawOver2 * sinPitchOver2 * sinRollOver2;
            var z = cosYawOver2 * cosPitchOver2 * sinRollOver2 - sinYawOver2 * sinPitchOver2 * cosRollOver2;

            return new SDKQuaternion() { x = x, y = y, z = z, w = w };
        }

        public static SDKQuaternion operator *(SDKQuaternion lhs, SDKQuaternion rhs)
        {
            float tx = lhs.w * rhs.x + lhs.x * rhs.w + lhs.y * rhs.z - lhs.z * rhs.y;
            float ty = lhs.w * rhs.y + lhs.y * rhs.w + lhs.z * rhs.x - lhs.x * rhs.z;
            float tz = lhs.w * rhs.z + lhs.z * rhs.w + lhs.x * rhs.y - lhs.y * rhs.x;
            float tw = lhs.w * rhs.w - lhs.x * rhs.x - lhs.y * rhs.y - lhs.z * rhs.z;

            return new SDKQuaternion() { x = tx, y = ty, z = tz, w = tw };
        }

        public static SDKVector3 operator *(SDKQuaternion lhs, SDKVector3 rhs)
        {
            float tx = lhs.x * 2.0f;
            float ty = lhs.y * 2.0f;
            float tz = lhs.z * 2.0f;
            float txx = lhs.x * tx;
            float tyy = lhs.y * ty;
            float tzz = lhs.z * tz;
            float txy = lhs.x * ty;
            float txz = lhs.x * tz;
            float tyz = lhs.y * tz;
            float twx = lhs.w * tx;
            float twy = lhs.w * ty;
            float twz = lhs.w * tz;

            SDKVector3 res;
            res.x = (1.0f - (tyy + tzz)) * rhs.x + (txy - twz) * rhs.y + (txz + twy) * rhs.z;
            res.y = (txy + twz) * rhs.x + (1.0f - (txx + tzz)) * rhs.y + (tyz - twx) * rhs.z;
            res.z = (txz - twy) * rhs.x + (tyz + twx) * rhs.y + (1.0f - (txx + tyy)) * rhs.z;
            return res;
        }
        // Delete end
        public override string ToString()
        {
            return
$@"SDKQuaternion:
x: {x}
y: {y}
z: {z}
w: {w}";
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDKMatrix4X4
    {
        public float m00, m01, m02, m03,
                        m10, m11, m12, m13,
                        m20, m21, m22, m23,
                        m30, m31, m32, m33;

        public static SDKMatrix4X4 Identity {
            get {
                return new SDKMatrix4X4()
                {
                    m00 = 1,
                    m01 = 0,
                    m02 = 0,
                    m03 = 0,
                    m10 = 0,
                    m11 = 1,
                    m12 = 0,
                    m13 = 0,
                    m20 = 0,
                    m21 = 0,
                    m22 = 1,
                    m23 = 0,
                    m30 = 0,
                    m31 = 0,
                    m32 = 0,
                    m33 = 1
                };
            }
        }

        public static implicit operator Matrix4x4(SDKMatrix4X4 v)
        {
            return new Matrix4x4()
            {
                m00 = v.m00,
                m01 = v.m01,
                m02 = v.m02,
                m03 = v.m03,
                m10 = v.m10,
                m11 = v.m11,
                m12 = v.m12,
                m13 = v.m13,
                m20 = v.m20,
                m21 = v.m21,
                m22 = v.m22,
                m23 = v.m23,
                m30 = v.m30,
                m31 = v.m31,
                m32 = v.m32,
                m33 = v.m33
            };
        }

        public static implicit operator SDKMatrix4X4(Matrix4x4 v)
        {
            return new SDKMatrix4X4()
            {
                m00 = v.m00,
                m01 = v.m01,
                m02 = v.m02,
                m03 = v.m03,
                m10 = v.m10,
                m11 = v.m11,
                m12 = v.m12,
                m13 = v.m13,
                m20 = v.m20,
                m21 = v.m21,
                m22 = v.m22,
                m23 = v.m23,
                m30 = v.m30,
                m31 = v.m31,
                m32 = v.m32,
                m33 = v.m33
            };
        }

        // TODO: Document change. Mathf.Deg2Rad and Methf.PI were stripped by IL2cpp so I hardcoded it here.
        private static float deg2Rad = (3.14159265358979f * 2f) / 360f;
        public static SDKMatrix4X4 Perspective(float vFov, float aspect, float zNear, float zFar)
        {
            float vFovRad = vFov * deg2Rad;
            float hFovRad = 2.0f * Mathf.Atan(Mathf.Tan(vFovRad * 0.5f) * aspect);
            float w = 1.0f / Mathf.Tan(hFovRad * 0.5f);
            float h = 1.0f / Mathf.Tan(vFovRad * 0.5f);
            float q0 = (zFar + zNear) / (zNear - zFar);
            float q1 = 2.0f * (zFar * zNear) / (zNear - zFar);

            return new SDKMatrix4X4()
            {
                m00 = w,
                m01 = 0,
                m02 = 0,
                m03 = 0,
                m10 = 0,
                m11 = h,
                m12 = 0,
                m13 = 0,
                m20 = 0,
                m21 = 0,
                m22 = q0,
                m23 = q1,
                m30 = 0,
                m31 = 0,
                m32 = -1,
                m33 = 0
            };
        }

        // begin delete
        public static SDKMatrix4X4 operator *(SDKMatrix4X4 lhs, SDKMatrix4X4 rhs)
        {
            SDKMatrix4X4 res = Identity;

            res.m00 = lhs.m00 * rhs.m00 + lhs.m01 * rhs.m10 + lhs.m02 * rhs.m20 + lhs.m03 * rhs.m30;
            res.m01 = lhs.m00 * rhs.m01 + lhs.m01 * rhs.m11 + lhs.m02 * rhs.m21 + lhs.m03 * rhs.m31;
            res.m02 = lhs.m00 * rhs.m02 + lhs.m01 * rhs.m12 + lhs.m02 * rhs.m22 + lhs.m03 * rhs.m32;
            res.m03 = lhs.m00 * rhs.m03 + lhs.m01 * rhs.m13 + lhs.m02 * rhs.m23 + lhs.m03 * rhs.m33;

            res.m10 = lhs.m10 * rhs.m00 + lhs.m11 * rhs.m10 + lhs.m12 * rhs.m20 + lhs.m13 * rhs.m30;
            res.m11 = lhs.m10 * rhs.m01 + lhs.m11 * rhs.m11 + lhs.m12 * rhs.m21 + lhs.m13 * rhs.m31;
            res.m12 = lhs.m10 * rhs.m02 + lhs.m11 * rhs.m12 + lhs.m12 * rhs.m22 + lhs.m13 * rhs.m32;
            res.m13 = lhs.m10 * rhs.m03 + lhs.m11 * rhs.m13 + lhs.m12 * rhs.m23 + lhs.m13 * rhs.m33;

            res.m20 = lhs.m20 * rhs.m00 + lhs.m21 * rhs.m10 + lhs.m22 * rhs.m20 + lhs.m23 * rhs.m30;
            res.m21 = lhs.m20 * rhs.m01 + lhs.m21 * rhs.m11 + lhs.m22 * rhs.m21 + lhs.m23 * rhs.m31;
            res.m22 = lhs.m20 * rhs.m02 + lhs.m21 * rhs.m12 + lhs.m22 * rhs.m22 + lhs.m23 * rhs.m32;
            res.m23 = lhs.m20 * rhs.m03 + lhs.m21 * rhs.m13 + lhs.m22 * rhs.m23 + lhs.m23 * rhs.m33;

            res.m30 = lhs.m30 * rhs.m00 + lhs.m31 * rhs.m10 + lhs.m32 * rhs.m20 + lhs.m33 * rhs.m30;
            res.m31 = lhs.m30 * rhs.m01 + lhs.m31 * rhs.m11 + lhs.m32 * rhs.m21 + lhs.m33 * rhs.m31;
            res.m32 = lhs.m30 * rhs.m02 + lhs.m31 * rhs.m12 + lhs.m32 * rhs.m22 + lhs.m33 * rhs.m32;
            res.m33 = lhs.m30 * rhs.m03 + lhs.m31 * rhs.m13 + lhs.m32 * rhs.m23 + lhs.m33 * rhs.m33;
            return res;
        }

        public static SDKVector3 operator *(SDKMatrix4X4 lhs, SDKVector3 rhs)
        {
            SDKVector3 res;
            res.x = lhs.m00 * rhs.x + lhs.m01 * rhs.y + lhs.m02 * rhs.z;
            res.y = lhs.m10 * rhs.x + lhs.m11 * rhs.y + lhs.m12 * rhs.z;
            res.z = lhs.m20 * rhs.x + lhs.m21 * rhs.y + lhs.m22 * rhs.z;
            return res;
        }

        // Creates a translation matrix.
        public static SDKMatrix4X4 Translate(SDKVector3 value)
        {
            return new SDKMatrix4X4
            {
                m00 = 1.0f,
                m01 = 0.0f,
                m02 = 0.0f,
                m03 = value.x,
                m10 = 0.0f,
                m11 = 1.0f,
                m12 = 0.0f,
                m13 = value.y,
                m20 = 0.0f,
                m21 = 0.0f,
                m22 = 1.0f,
                m23 = value.z,
                m30 = 0.0f,
                m31 = 0.0f,
                m32 = 0.0f,
                m33 = 1.0f
            };
        }

        // Creates a rotation matrix.
        public static SDKMatrix4X4 Rotate(SDKQuaternion value)
        {
            float qx = value.x;
            float qy = value.y;
            float qz = value.z;
            float qw = value.w;

            return new SDKMatrix4X4
            {
                m00 = 1.0f - 2.0f * qy * qy - 2.0f * qz * qz,
                m01 = 2.0f * qx * qy - 2.0f * qz * qw,
                m02 = 2.0f * qx * qz + 2.0f * qy * qw,
                m03 = 0.0f,
                m10 = 2.0f * qx * qy + 2.0f * qz * qw,
                m11 = 1.0f - 2.0f * qx * qx - 2.0f * qz * qz,
                m12 = 2.0f * qy * qz - 2.0f * qx * qw,
                m13 = 0.0f,
                m20 = 2.0f * qx * qz - 2.0f * qy * qw,
                m21 = 2.0f * qy * qz + 2.0f * qx * qw,
                m22 = 1.0f - 2.0f * qx * qx - 2.0f * qy * qy,
                m23 = 0.0f,
                m30 = 0.0f,
                m31 = 0.0f,
                m32 = 0.0f,
                m33 = 1.0f
            };
        }

        // Creates a scaling matrix.
        public static SDKMatrix4X4 Scale(SDKVector3 value)
        {
            return new SDKMatrix4X4
            {
                m00 = value.x,
                m01 = 0.0f,
                m02 = 0.0f,
                m03 = 0.0f,
                m10 = 0.0f,
                m11 = value.y,
                m12 = 0.0f,
                m13 = 0.0f,
                m20 = 0.0f,
                m21 = 0.0f,
                m22 = value.z,
                m23 = 0.0f,
                m30 = 0.0f,
                m31 = 0.0f,
                m32 = 0.0f,
                m33 = 1.0f
            };
        }

        public static SDKMatrix4X4 TRS(SDKVector3 translation, SDKQuaternion rotation, SDKVector3 scale)
        {
            return Translate(translation) * Rotate(rotation) * Scale(scale);
        }
        // end delete

        public override string ToString()
        {
            return
$@"Matrix4x4:
{m00} {m01} {m02} {m03}
{m10} {m11} {m12} {m13}
{m20} {m21} {m22} {m23}
{m30} {m31} {m32} {m33}";
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDKPlane 
    {
        public float distance;
        public SDKVector3 normal;

        public static implicit operator SDKPlane(Plane v)
        {
            return new SDKPlane()
            {
                distance = v.distance,
                normal = v.normal
            };
        }

        public static SDKPlane Empty {
            get {
                return new SDKPlane() { distance = 0f, normal = SDKVector3.Up };
            }
        }

        public override string ToString()
        {
            return
$@"SDKPlane:
{distance} {normal}";
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDKPriority
    {
        public sbyte pose;
        public sbyte clipPlane;
        public sbyte stage;
        public sbyte resolution;
        public sbyte feature;
        public sbyte nearFarAdjustment;
        public sbyte groundPlane;
        public sbyte reserved2;

        public static SDKPriority Empty {
            get {
                return new SDKPriority()
                {
                    pose = -(sbyte)Priority.Game,
                    clipPlane = -(sbyte)Priority.Game,
                    stage = -(sbyte)Priority.Game,
                    resolution = -(sbyte)Priority.Game,
                    feature = -(sbyte)Priority.Game,
                    nearFarAdjustment = (sbyte)Priority.Game,
                    groundPlane = -(sbyte)Priority.Game,
                    reserved2 = -(sbyte)Priority.Game
                };
            }
        }

        public override string ToString()
        {
            return
$@"Priority:
pose: {pose}, clipPlane: {clipPlane}, stage: {stage}, resolution: {resolution}, feature: {feature}, nearFarAdjustment: {nearFarAdjustment}, groundPlane: {groundPlane}";
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDKApplicationOutput
    {
        public Features supportedFeatures;
        public string engineName;
        public string engineVersion;
        public string applicationName;
        public string applicationVersion;
        public string xrDeviceName;
        public string graphicsAPI;
        public string sdkID;
        public string sdkVersion;

        public static SDKApplicationOutput Empty {
            get {
                return new SDKApplicationOutput()
                {
                    supportedFeatures = Features.None,
                    engineName = string.Empty,
                    engineVersion = string.Empty,
                    applicationName = string.Empty,
                    applicationVersion = string.Empty,
                    xrDeviceName = string.Empty,
                    graphicsAPI = string.Empty,
                    sdkID = SDKConstants.SDKID,
                    sdkVersion = string.Empty
                };
            }
        }

        public override string ToString()
        {
            return
$@"SDKApplicationOutput:
supportedFeatures: {supportedFeatures}
engineName: {engineName}
engineVersion: {engineVersion}
applicationName: {applicationName}
applicationVersion: {applicationVersion}
xrDeviceName: {xrDeviceName}
graphicsAPI: {graphicsAPI}
sdkID: {sdkID}
sdkVersion: {sdkVersion}";
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDKInputFrame
    {
        public SDKPose pose;
        public SDKClipPlane clipPlane;
        public SDKTransform stageTransform;
        public Features features;
        public SDKClipPlane groundClipPlane;

        public ulong frameid; // This is actually the time stamp of this frame - its populated by the bridge at creation time.
        public ulong referenceframe; // Use the previous frames frameid to populate this field - it must be set to the correct frame id, or it will fail. 
        public SDKPriority priority; // this is a mixed field combining flags and priority - the contents of this flag are not yet set in stone            

        public static SDKInputFrame Empty {
            get {
                return new SDKInputFrame()
                {
                    pose = SDKPose.Empty,
                    clipPlane = SDKClipPlane.Empty,
                    stageTransform = SDKTransform.Empty,
                    features = Features.None,
                    groundClipPlane = SDKClipPlane.Empty,
                    frameid = 0,
                    referenceframe = 0,
                    priority = SDKPriority.Empty
                };
            }
        }

        public void ReleaseControl()
        {
            priority = SDKPriority.Empty;
        }

        public void ObtainControl()
        {
            priority = SDKPriority.Empty;
            priority.pose = (sbyte)Priority.Game;
        }

        public override string ToString()
        {
            return
$@"SDKInputFrame:
pose: {pose}
clipPlane: {clipPlane}
stageTransform: {stageTransform}
features: {features}
groundClipPlane: {groundClipPlane}
frameid: {frameid}
referenceframe: {referenceframe}
priority: {priority:X4}";
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDKOutputFrame
    {
        public RenderingPipeline renderingPipeline;
        public SDKTrackedSpace trackedSpace;

        public static SDKOutputFrame Empty {
            get {
                return new SDKOutputFrame()
                {
                    renderingPipeline = RenderingPipeline.Undefined,
                    trackedSpace = SDKTrackedSpace.Empty
                };
            }
        }

        public override string ToString()
        {
            return
$@"SDKOutputFrame:
renderingPipeline: {renderingPipeline}
trackedSpace: {trackedSpace}";
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDKTrackedSpace
    {
        public SDKVector3 trackedSpaceWorldPosition;
        public SDKQuaternion trackedSpaceWorldRotation;
        public SDKVector3 trackedSpaceLocalScale;
        public SDKMatrix4X4 trackedSpaceLocalToWorldMatrix;
        public SDKMatrix4X4 trackedSpaceWorldToLocalMatrix;

        public static SDKTrackedSpace Empty {
            get {
                return new SDKTrackedSpace()
                {
                    trackedSpaceWorldPosition = SDKVector3.Zero,
                    trackedSpaceWorldRotation = SDKQuaternion.Identity,
                    trackedSpaceLocalScale = SDKVector3.Zero,
                    trackedSpaceLocalToWorldMatrix = SDKMatrix4X4.Identity,
                    trackedSpaceWorldToLocalMatrix = SDKMatrix4X4.Identity
                };
            }
        }

        public override string ToString()
        {
            return
$@"SDKTrackedSpace:
trackedSpaceWorldPosition: {trackedSpaceWorldPosition}
trackedSpaceWorldRotation: {trackedSpaceWorldRotation}
trackedSpaceLocalScale: {trackedSpaceLocalScale}
trackedSpaceLocalToWorldMatrix: {trackedSpaceLocalToWorldMatrix}
trackedSpaceWorldToLocalMatrix: {trackedSpaceWorldToLocalMatrix}";
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDKTexture
    {
        public TextureID id;
        public IntPtr texturePtr;
        public IntPtr SharedHandle;
        public TextureDevice device;
        public int dummy;
        public TextureType type;
        public TextureFormat format;
        public TextureColorSpace colorSpace;
        public int width;
        public int height;

        public static SDKTexture Empty {
            get {
                return new SDKTexture()
                {
                    id = TextureID.Undefined,
                    texturePtr = IntPtr.Zero,
                    SharedHandle = IntPtr.Zero,
                    device = TextureDevice.Undefined,
                    dummy = 0,
                    type = TextureType.Undefined,
                    format = TextureFormat.Undefined,
                    colorSpace = TextureColorSpace.Undefined,
                    width = 0,
                    height = 0
                };
            }
        }

        public override string ToString()
        {
            return
$@"SDKTexture:
id: {id}
texturePtr: {texturePtr}
SharedHandle: {SharedHandle}
device: {device}
dummy: {dummy}
type: {type}
format: {format}
colorSpace: {colorSpace}
width: {width}
height: {height}";
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDKTransform
    {
        public SDKVector3 localPosition;
        public SDKQuaternion localRotation;
        public SDKVector3 localScale;

        public static SDKTransform Empty {
            get {
                return new SDKTransform()
                {
                    localPosition = SDKVector3.Zero,
                    localRotation = SDKQuaternion.Identity,
                    localScale = SDKVector3.One
                };
            }
        }

        public override string ToString()
        {
            return
$@"SDKTransform:
localPosition: {localPosition}
localRotation: {localRotation}
localScale: {localScale}";
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDKClipPlane
    {
        public SDKMatrix4X4 transform;
        public int width;
        public int height;
        public float tesselation;

        public static SDKClipPlane Empty {
            get {
                return new SDKClipPlane()
                {
                    transform = SDKMatrix4X4.Identity,
                    width = 0,
                    height = 0,
                    tesselation = 0
                };
            }
        }

        public override string ToString()
        {
            return
$@"SDKClipPlane:
transform: {transform}
width: {width}
height: {height}
tesselation: {tesselation}";
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDKControllerState
    {
        public SDKVector3 hmdposition;
        public SDKQuaternion hmdrotation;

        public SDKVector3 calibrationcameraposition;
        public SDKQuaternion calibrationcamerarotation;

        public SDKVector3 cameraposition;
        public SDKQuaternion camerarotation;

        public SDKVector3 leftposition;
        public SDKQuaternion leftrotation;

        public SDKVector3 rightposition;
        public SDKQuaternion rightrotation;

        public static SDKControllerState Empty {
            get {
                return new SDKControllerState()
                {
                    hmdposition = SDKVector3.Zero,
                    hmdrotation = SDKQuaternion.Identity,
                    calibrationcameraposition = SDKVector3.Zero,
                    calibrationcamerarotation = SDKQuaternion.Identity,
                    cameraposition = SDKVector3.Zero,
                    camerarotation = SDKQuaternion.Identity,
                    leftposition = SDKVector3.Zero,
                    leftrotation = SDKQuaternion.Identity,
                    rightposition = SDKVector3.Zero,
                    rightrotation = SDKQuaternion.Identity,
                };
            }
        }

        public override string ToString()
        {
            return
$@"SDKControllerState:
hmdposition: {hmdposition}
hmdrotation: {hmdrotation}
calibrationcameraposition: {calibrationcameraposition}
calibrationcamerarotation: {calibrationcamerarotation}
cameraposition: {cameraposition}
camerarotation: {camerarotation}
leftposition: {leftposition}
leftrotation: {leftrotation}
rightposition: {rightposition}
rightrotation: {rightrotation}";
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDKPose
    {
        public SDKMatrix4X4 projectionMatrix;
        public SDKVector3 localPosition;
        public SDKQuaternion localRotation;
        public float verticalFieldOfView;
        public float nearClipPlane;
        public float farClipPlane;
        public int unused0;
        public int unused1;

        public static SDKPose Empty {
            get {
                return new SDKPose()
                {
                    projectionMatrix = SDKMatrix4X4.Perspective(90f, 1f, 0.01f, 1000f),
                    localPosition = SDKVector3.Zero,
                    localRotation = SDKQuaternion.Identity,                    
                    verticalFieldOfView = 90f,
                    nearClipPlane = 0.01f,
                    farClipPlane = 1000f,
                };
            }
        }

        public override string ToString()
        {
            return
$@"SDKPose:
projectionMatrix: {projectionMatrix}
localPosition: {localPosition}
localRotation: {localRotation}
verticalFieldOfView: {verticalFieldOfView}
nearClipPlane: {nearClipPlane}
farClipPlane: {farClipPlane}";
        }
    }
}