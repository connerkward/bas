// Copyright (c) 2021 homuler
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using System;
using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;

namespace Mediapipe
{
  internal static partial class SafeNativeMethods
  {
    [Pure, DllImport(MediaPipeLibrary, ExactSpelling = true)]
    public static extern int mp_GpuBuffer__width(IntPtr gpuBuffer);

    [Pure, DllImport(MediaPipeLibrary, ExactSpelling = true)]
    public static extern int mp_GpuBuffer__height(IntPtr gpuBuffer);

    [Pure, DllImport(MediaPipeLibrary, ExactSpelling = true)]
    public static extern GpuBufferFormat mp_GpuBuffer__format(IntPtr gpuBuffer);
  }
}
