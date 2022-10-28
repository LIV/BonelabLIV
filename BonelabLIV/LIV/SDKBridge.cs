using System;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

namespace LIV.SDK.Unity
{
	public static class SDKBridge
	{
		private const string DllPath = "LIV_Bridge.dll";

#if (UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN) && UNITY_64

		#region Interop

		[DllImport(DllPath)]
		private static extern IntPtr GetRenderEventFunc();

		[DllImport(DllPath, EntryPoint = "LivCaptureIsActive")]
		[return: MarshalAs(UnmanagedType.U1)]
		private static extern bool GetIsCaptureActive();

		[DllImport(DllPath, EntryPoint = "LivCaptureWidth")]
		private static extern int GetTextureWidth();

		[DllImport(DllPath, EntryPoint = "LivCaptureHeight")]
		private static extern int GetTextureHeight();

		[DllImport(DllPath, EntryPoint = "LivCaptureSetTextureFromUnity")]
		private static extern void SetTexture(IntPtr texture);

		//// Acquire a frame from the compositor, allowing atomic access to its properties - most current one by default
		[DllImport(DllPath, EntryPoint = "AcquireCompositorFrame")]
		public static extern int AcquireCompositorFrame(ulong timestamp);

		[DllImport(DllPath, EntryPoint = "ReleaseCompositorFrame")]
		public static extern int ReleaseCompositorFrame();

		// Get timestamp of SDK2 object (C# timestamp) - must be an object in the bridge, not a copy.
		[DllImport(DllPath, EntryPoint = "GetObjectTimeStamp")]
		public static extern ulong GetObjectTimeStamp(IntPtr obj);

		// Get current time in C# ticks
		[DllImport(DllPath, EntryPoint = "GetCurrentTimeTicks")]
		private static extern ulong GetCurrentTimeTicks();

		// Get object tag of SDK2 object - must be an object in the bridge, not a copy.
		[DllImport(DllPath, EntryPoint = "GetObjectTag")]
		public static extern ulong GetObjectTag(IntPtr obj);

		// Get a frame object from the compositor
		[DllImport(DllPath, EntryPoint = "GetCompositorFrameObject")]
		public static extern IntPtr GetCompositorFrameObject(ulong tag);

		// Get a frame object from the compositor
		[DllImport(DllPath, EntryPoint = "GetViewportTexture")]
		public static extern IntPtr GetViewportTexture();

		// Get a channel object from the compositor
		[DllImport(DllPath, EntryPoint = "GetCompositorChannelObject")]
		public static extern IntPtr GetCompositorChannelObject(int slot, ulong tag, ulong timestamp);

		// Get a channel object from our own source channel
		[DllImport(DllPath, EntryPoint = "GetChannelObject")]
		public static extern IntPtr GetChannelObject(int slot, ulong tag, ulong timestamp);

		// Write an object to our channel
		[DllImport(DllPath, EntryPoint = "AddObjectToChannel")]
		public static extern int AddObjectToChannel(int slot, IntPtr obj, int objectsize, ulong tag);

		// Write an object to the compostor's channel
		[DllImport(DllPath, EntryPoint = "AddObjectToCompositorChannel")]
		public static extern int AddObjectToCompositorChannel(int slot, IntPtr obj, int objectsize, ulong tag);

		// Add a structure/object to the current frame / Considering if its simpler to combine with AddObjectToChannel with 0 being the frame
		[DllImport(DllPath, EntryPoint = "AddObjectToFrame")]
		public static extern int AddObjectToFrame(IntPtr obj, int objectsize, ulong tag);

		// Helper to add strings 
		[DllImport(DllPath, EntryPoint = "AddObjectToFrame")]
		public static extern int AddStringToFrame(IntPtr str, ulong tag);

		[DllImport(DllPath, EntryPoint = "AddStringToChannel")]
		public static extern int AddStringToChannel(int slot, IntPtr str, int length, ulong tag);

		// Create a new frame for rendering / native code does this already - so probably don't use
		[DllImport(DllPath, EntryPoint = "NewFrame")]
		public static extern int NewFrame();

		// Commit the frame early - not recommended - best to let the next NewFrame commit the frame to avoid pipeline stalls
		[DllImport(DllPath, EntryPoint = "CommitFrame")]
		public static extern IntPtr CommitFrame();

		// Add a copy of a unity texture to the bridge
		[DllImport(DllPath, EntryPoint = "addsharedtexture")]
		public static extern int addsharedtexture(int width, int height, int format, IntPtr sourcetexture, ulong tag);

		[DllImport(DllPath, EntryPoint = "addtexture")]
		public static extern int addtexture(IntPtr sourcetexture, ulong tag);

		[DllImport(DllPath, EntryPoint = "PublishTextures")]
		public static extern void PublishTextures();

		[DllImport(DllPath, EntryPoint = "updateinputframe")]
		public static extern IntPtr updatinputframe(IntPtr InputFrame);

		[DllImport(DllPath, EntryPoint = "setinputframe")]
		public static extern IntPtr setinputframe(float x, float y, float z, float q0, float q1, float q2, float q3, float fov, int priority);

		[DllImport(DllPath, EntryPoint = "setfeature")]
		public static extern ulong setfeature(ulong feature);

		[DllImport(DllPath, EntryPoint = "clearfeature")]
		public static extern ulong clearfeature(ulong feature);

		#endregion

#else
        public static int AddStringToChannel(int slot, IntPtr str, int length, ulong tag) { return -2; }
        public static int addtexture(IntPtr sourcetexture, ulong tag) { return -2; }
        public static ulong GetObjectTimeStamp(IntPtr obj) { return 0; }
        public static ulong GetCurrentTimeTicks() { return 0; }
        static bool GetIsCaptureActive() { return false; }
        public static IntPtr GetRenderEventFunc() { return IntPtr.Zero; }
        public static IntPtr GetCompositorChannelObject(int slot, ulong tag, ulong timestamp) { return IntPtr.Zero; }
        public static int AddObjectToCompositorChannel(int slot, IntPtr obj, int objectsize, ulong tag) { return -2; }
        public static int AddObjectToFrame(IntPtr obj, int objectsize, ulong tag) { return -2; }
        public static IntPtr updatinputframe(IntPtr InputFrame) { return IntPtr.Zero; }
        public static IntPtr GetViewportTexture() { return IntPtr.Zero; }
        public static IntPtr GetChannelObject(int slot, ulong tag, ulong timestamp) { return IntPtr.Zero; }
        public static int AddObjectToChannel(int slot, IntPtr obj, int objectsize, ulong tag) { return -2; }
#endif

		public struct SDKInjection<T>
		{
			public bool Active;
			public Action Action;
			public T Data;
		}

		private static SDKInjection<SDKInputFrame> _injectionSDKInputFrame = new SDKInjection<SDKInputFrame>
		{
			Active = false,
			Action = null,
			Data = SDKInputFrame.Empty
		};

		private static SDKInjection<SDKResolution> _injectionSDKResolution = new SDKInjection<SDKResolution>
		{
			Active = false,
			Action = null,
			Data = SDKResolution.Zero
		};

		private static readonly SDKInjection<bool> InjectionIsActive = new SDKInjection<bool>
		{
			Active = false,
			Action = null,
			Data = false
		};

		private static readonly bool InjectionDisableSubmit = false;
		private static readonly bool InjectionDisableSubmitApplicationOutput = false;
		private static readonly bool InjectionDisableAddTexture = false;
		private static readonly bool InjectionDisableCreateFrame = false;

		// Get the tag code for a string - won't win any awards - pre-compute these and use constants.
		public static ulong Tag(string str)
		{
			ulong tag = 0;
			for (var i = 0; i < str.Length; i++)
			{
				if (i == 8) break;
				var c = str[i];
				tag |= (ulong) (c & 255) << i * 8;
			}

			return tag;
		}

		public static void AddString(string tag, string value, int slot)
		{
			var utf8 = Encoding.UTF8;
			var utfBytes = utf8.GetBytes(value);
			var gch = GCHandle.Alloc(utfBytes, GCHandleType.Pinned);
			AddStringToChannel(slot, Marshal.UnsafeAddrOfPinnedArrayElement(utfBytes, 0), utfBytes.Length, Tag(tag));
			gch.Free();
		}

		public static void AddTexture(SDKTexture texture, ulong tag)
		{
			var gch = GCHandle.Alloc(texture, GCHandleType.Pinned);
			addtexture(gch.AddrOfPinnedObject(), tag);
			gch.Free();
		}

		public static ulong GetObjectTime(IntPtr objectptr)
		{
			return GetObjectTimeStamp(objectptr) + 621355968000000000;
		}

		public static ulong GetCurrentTime()
		{
			return GetCurrentTimeTicks() + 621355968000000000;
		}

		public static bool IsActive
		{
			get
			{
				if (InjectionIsActive.Active)
				{
					return InjectionIsActive.Data;
				}
				return GetIsCaptureActive();
			}
		}

		public static void IssuePluginEvent()
		{
			if (InjectionDisableSubmit) return;
			GL.IssuePluginEvent(GetRenderEventFunc(), 2);
		}

		public static void SubmitApplicationOutput(SDKApplicationOutput applicationOutput)
		{
			if (InjectionDisableSubmitApplicationOutput) return;
			AddString("APPNAME", applicationOutput.applicationName, 5);
			AddString("APPVER", applicationOutput.applicationVersion, 5);
			AddString("ENGNAME", applicationOutput.engineName, 5);
			AddString("ENGVER", applicationOutput.engineVersion, 5);
			AddString("GFXAPI", applicationOutput.graphicsAPI, 5);
			AddString("SDKID", applicationOutput.sdkID, 5);
			AddString("SDKVER", applicationOutput.sdkVersion, 5);
			AddString("SUPPORT", applicationOutput.supportedFeatures.ToString(), 5);
			AddString("XRNAME", applicationOutput.xrDeviceName, 5);
		}

		public static bool GetStructFromGlobalChannel<T>(ref T mystruct, int channel, ulong tag)
		{
			var structPtr = GetCompositorChannelObject(channel, tag, ulong.MaxValue);
			if (structPtr == IntPtr.Zero) return false;
			mystruct = (T) Marshal.PtrToStructure(structPtr, typeof(T));
			return true;
		}

		public static int AddStructToGlobalChannel<T>(ref T mystruct, int channel, ulong tag)
		{
			var gch = GCHandle.Alloc(mystruct, GCHandleType.Pinned);
			var output = AddObjectToCompositorChannel(channel, gch.AddrOfPinnedObject(), Marshal.SizeOf(mystruct), tag);
			gch.Free();
			return output;
		}

		public static bool GetStructFromLocalChannel<T>(ref T mystruct, int channel, ulong tag)
		{
			var structPtr = GetChannelObject(channel, tag, ulong.MaxValue);
			if (structPtr == IntPtr.Zero) return false;
			mystruct = (T) Marshal.PtrToStructure(structPtr, typeof(T));
			return true;
		}

		public static int AddStructToLocalChannel<T>(ref T mystruct, int channel, ulong tag)
		{
			var gch = GCHandle.Alloc(mystruct, GCHandleType.Pinned);
			var output = AddObjectToChannel(channel, gch.AddrOfPinnedObject(), Marshal.SizeOf(mystruct), tag);
			gch.Free();
			return output;
		}

		// Add ANY structure to the current frame
		public static void AddStructToFrame<T>(ref T mystruct, ulong tag)
		{
			var gch = GCHandle.Alloc(mystruct, GCHandleType.Pinned);
			AddObjectToFrame(gch.AddrOfPinnedObject(), Marshal.SizeOf(mystruct), tag);
			gch.Free();
		}


		/// <summary>
		///   Update the master pose sent to ALL applications.
		///   when called initialy, having the flags set to 0 will return the current pose (which includes resolution - which you
		///   might need)
		///   If you wish to change the pose, change the parts of the structures you need to, and set the appropriate flag to
		///   update.
		///   atm, the flags will be for Pose, Stage, Clipping Plane, and resolution.
		/// </summary>
		/// <param name="setframe"></param>
		/// <returns>The current pose - could be yours, someone elses, or a combination</returns>
		public static bool UpdateInputFrame(ref SDKInputFrame setframe)
		{
			if (_injectionSDKInputFrame.Active && _injectionSDKInputFrame.Action != null)
			{
				_injectionSDKInputFrame.Action.Invoke();
				setframe = _injectionSDKInputFrame.Data;
			}
			else
			{
				// Pin the object briefly so we can send it to the API without it being accidentally garbage collected
				var gch = GCHandle.Alloc(setframe, GCHandleType.Pinned);
				var structPtr = updatinputframe(gch.AddrOfPinnedObject());
				gch.Free();

				if (structPtr == IntPtr.Zero)
				{
					setframe = SDKInputFrame.Empty;
					return false;
				}

				setframe = (SDKInputFrame) Marshal.PtrToStructure(structPtr, typeof(SDKInputFrame));
				_injectionSDKInputFrame.Data = setframe;
			}

			return true;
		}

		public static SDKTexture GetViewfinderTexture()
		{
			var overlaytexture = SDKTexture.Empty;
			var structPtr = GetCompositorChannelObject(11, Tag("OUTTEX"), ulong.MaxValue);
			if (structPtr == IntPtr.Zero) return new SDKTexture();
			overlaytexture = (SDKTexture) Marshal.PtrToStructure(structPtr, typeof(SDKTexture));
			return overlaytexture;
		}

		public static void AddTexture(SDKTexture texture)
		{
			if (InjectionDisableAddTexture) return;
			var tag = "";
			switch (texture.id)
			{
				case TextureID.BackgroundColorBufferID:
					tag = "BGCTEX";
					break;
				case TextureID.ForegroundColorBufferID:
					tag = "FGCTEX";
					break;
				case TextureID.OptimizedColorBufferID:
					tag = "OPTTEX";
					break;
			}
			AddTexture(texture, Tag(tag));
		}

		public static void CreateFrame(SDKOutputFrame frame)
		{
			if (InjectionDisableCreateFrame) return;
			var gch = GCHandle.Alloc(frame, GCHandleType.Pinned);
			AddObjectToFrame(gch.AddrOfPinnedObject(), Marshal.SizeOf(frame), Tag("OUTFRAME"));
			gch.Free();
		}

		public static void SetGroundPlane(SDKPlane groundPlane)
		{
			AddStructToGlobalChannel(ref groundPlane, 2, Tag("SetGND"));
		}

		public static bool GetResolution(ref SDKResolution sdkResolution)
		{
			if (_injectionSDKResolution.Active && _injectionSDKResolution.Action != null)
			{
				_injectionSDKResolution.Action.Invoke();
				sdkResolution = _injectionSDKResolution.Data;
				return true;
			}

			var output = GetStructFromLocalChannel(ref sdkResolution, 15, Tag("SDKRes"));
			_injectionSDKResolution.Data = sdkResolution;
			return output;
		}
	}
}