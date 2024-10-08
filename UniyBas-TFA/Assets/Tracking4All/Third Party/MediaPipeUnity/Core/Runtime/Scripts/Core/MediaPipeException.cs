// Copyright (c) 2021 homuler
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.
using System;

namespace Mediapipe
{
  public class MediaPipeException : Exception
  {
    public MediaPipeException(string message) : base(message) { }
  }
}
