// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: mediapipe/calculators/video/box_tracker_calculator.proto
// </auto-generated>
#pragma warning disable 1591, 0612, 3021
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
namespace Mediapipe {

  /// <summary>Holder for reflection information generated from mediapipe/calculators/video/box_tracker_calculator.proto</summary>
  public static partial class BoxTrackerCalculatorReflection {

    #region Descriptor
    /// <summary>File descriptor for mediapipe/calculators/video/box_tracker_calculator.proto</summary>
    public static pbr::FileDescriptor Descriptor {
      get { return descriptor; }
    }
    private static pbr::FileDescriptor descriptor;

    static BoxTrackerCalculatorReflection() {
      byte[] descriptorData = global::System.Convert.FromBase64String(
          string.Concat(
            "CjhtZWRpYXBpcGUvY2FsY3VsYXRvcnMvdmlkZW8vYm94X3RyYWNrZXJfY2Fs",
            "Y3VsYXRvci5wcm90bxIJbWVkaWFwaXBlGiRtZWRpYXBpcGUvZnJhbWV3b3Jr",
            "L2NhbGN1bGF0b3IucHJvdG8aKW1lZGlhcGlwZS91dGlsL3RyYWNraW5nL2Jv",
            "eF90cmFja2VyLnByb3RvIqgDChtCb3hUcmFja2VyQ2FsY3VsYXRvck9wdGlv",
            "bnMSNQoPdHJhY2tlcl9vcHRpb25zGAEgASgLMhwubWVkaWFwaXBlLkJveFRy",
            "YWNrZXJPcHRpb25zEjYKEGluaXRpYWxfcG9zaXRpb24YAiABKAsyHC5tZWRp",
            "YXBpcGUuVGltZWRCb3hQcm90b0xpc3QSJgoXdmlzdWFsaXplX3RyYWNraW5n",
            "X2RhdGEYAyABKAg6BWZhbHNlEh4KD3Zpc3VhbGl6ZV9zdGF0ZRgEIAEoCDoF",
            "ZmFsc2USJwoYdmlzdWFsaXplX2ludGVybmFsX3N0YXRlGAUgASgIOgVmYWxz",
            "ZRIqCh9zdHJlYW1pbmdfdHJhY2tfZGF0YV9jYWNoZV9zaXplGAYgASgFOgEw",
            "EiYKG3N0YXJ0X3Bvc190cmFuc2l0aW9uX2ZyYW1lcxgHIAEoBToBMDJVCgNl",
            "eHQSHC5tZWRpYXBpcGUuQ2FsY3VsYXRvck9wdGlvbnMY9KSUgAEgASgLMiYu",
            "bWVkaWFwaXBlLkJveFRyYWNrZXJDYWxjdWxhdG9yT3B0aW9ucw=="));
      descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
          new pbr::FileDescriptor[] { global::Mediapipe.CalculatorReflection.Descriptor, global::Mediapipe.BoxTrackerReflection.Descriptor, },
          new pbr::GeneratedClrTypeInfo(null, null, new pbr::GeneratedClrTypeInfo[] {
            new pbr::GeneratedClrTypeInfo(typeof(global::Mediapipe.BoxTrackerCalculatorOptions), global::Mediapipe.BoxTrackerCalculatorOptions.Parser, new[]{ "TrackerOptions", "InitialPosition", "VisualizeTrackingData", "VisualizeState", "VisualizeInternalState", "StreamingTrackDataCacheSize", "StartPosTransitionFrames" }, null, null, new pb::Extension[] { global::Mediapipe.BoxTrackerCalculatorOptions.Extensions.Ext }, null)
          }));
    }
    #endregion

  }
  #region Messages
  public sealed partial class BoxTrackerCalculatorOptions : pb::IMessage<BoxTrackerCalculatorOptions>
  #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      , pb::IBufferMessage
  #endif
  {
    private static readonly pb::MessageParser<BoxTrackerCalculatorOptions> _parser = new pb::MessageParser<BoxTrackerCalculatorOptions>(() => new BoxTrackerCalculatorOptions());
    private pb::UnknownFieldSet _unknownFields;
    private int _hasBits0;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pb::MessageParser<BoxTrackerCalculatorOptions> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::Mediapipe.BoxTrackerCalculatorReflection.Descriptor.MessageTypes[0]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public BoxTrackerCalculatorOptions() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public BoxTrackerCalculatorOptions(BoxTrackerCalculatorOptions other) : this() {
      _hasBits0 = other._hasBits0;
      trackerOptions_ = other.trackerOptions_ != null ? other.trackerOptions_.Clone() : null;
      initialPosition_ = other.initialPosition_ != null ? other.initialPosition_.Clone() : null;
      visualizeTrackingData_ = other.visualizeTrackingData_;
      visualizeState_ = other.visualizeState_;
      visualizeInternalState_ = other.visualizeInternalState_;
      streamingTrackDataCacheSize_ = other.streamingTrackDataCacheSize_;
      startPosTransitionFrames_ = other.startPosTransitionFrames_;
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public BoxTrackerCalculatorOptions Clone() {
      return new BoxTrackerCalculatorOptions(this);
    }

    /// <summary>Field number for the "tracker_options" field.</summary>
    public const int TrackerOptionsFieldNumber = 1;
    private global::Mediapipe.BoxTrackerOptions trackerOptions_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public global::Mediapipe.BoxTrackerOptions TrackerOptions {
      get { return trackerOptions_; }
      set {
        trackerOptions_ = value;
      }
    }

    /// <summary>Field number for the "initial_position" field.</summary>
    public const int InitialPositionFieldNumber = 2;
    private global::Mediapipe.TimedBoxProtoList initialPosition_;
    /// <summary>
    /// Initial position to be tracked. Can also be supplied as side packet or
    /// as input stream.
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public global::Mediapipe.TimedBoxProtoList InitialPosition {
      get { return initialPosition_; }
      set {
        initialPosition_ = value;
      }
    }

    /// <summary>Field number for the "visualize_tracking_data" field.</summary>
    public const int VisualizeTrackingDataFieldNumber = 3;
    private readonly static bool VisualizeTrackingDataDefaultValue = false;

    private bool visualizeTrackingData_;
    /// <summary>
    /// If set and VIZ stream is present, renders tracking data into the
    /// visualization.
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool VisualizeTrackingData {
      get { if ((_hasBits0 & 1) != 0) { return visualizeTrackingData_; } else { return VisualizeTrackingDataDefaultValue; } }
      set {
        _hasBits0 |= 1;
        visualizeTrackingData_ = value;
      }
    }
    /// <summary>Gets whether the "visualize_tracking_data" field is set</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool HasVisualizeTrackingData {
      get { return (_hasBits0 & 1) != 0; }
    }
    /// <summary>Clears the value of the "visualize_tracking_data" field</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void ClearVisualizeTrackingData() {
      _hasBits0 &= ~1;
    }

    /// <summary>Field number for the "visualize_state" field.</summary>
    public const int VisualizeStateFieldNumber = 4;
    private readonly static bool VisualizeStateDefaultValue = false;

    private bool visualizeState_;
    /// <summary>
    /// If set and VIZ stream is present, renders the box state
    /// into the visualization.
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool VisualizeState {
      get { if ((_hasBits0 & 2) != 0) { return visualizeState_; } else { return VisualizeStateDefaultValue; } }
      set {
        _hasBits0 |= 2;
        visualizeState_ = value;
      }
    }
    /// <summary>Gets whether the "visualize_state" field is set</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool HasVisualizeState {
      get { return (_hasBits0 & 2) != 0; }
    }
    /// <summary>Clears the value of the "visualize_state" field</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void ClearVisualizeState() {
      _hasBits0 &= ~2;
    }

    /// <summary>Field number for the "visualize_internal_state" field.</summary>
    public const int VisualizeInternalStateFieldNumber = 5;
    private readonly static bool VisualizeInternalStateDefaultValue = false;

    private bool visualizeInternalState_;
    /// <summary>
    /// If set and VIZ stream is present, renders the internal box state
    /// into the visualization.
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool VisualizeInternalState {
      get { if ((_hasBits0 & 4) != 0) { return visualizeInternalState_; } else { return VisualizeInternalStateDefaultValue; } }
      set {
        _hasBits0 |= 4;
        visualizeInternalState_ = value;
      }
    }
    /// <summary>Gets whether the "visualize_internal_state" field is set</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool HasVisualizeInternalState {
      get { return (_hasBits0 & 4) != 0; }
    }
    /// <summary>Clears the value of the "visualize_internal_state" field</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void ClearVisualizeInternalState() {
      _hasBits0 &= ~4;
    }

    /// <summary>Field number for the "streaming_track_data_cache_size" field.</summary>
    public const int StreamingTrackDataCacheSizeFieldNumber = 6;
    private readonly static int StreamingTrackDataCacheSizeDefaultValue = 0;

    private int streamingTrackDataCacheSize_;
    /// <summary>
    /// Size of the track data cache during streaming mode. This allows to buffer
    /// track_data's for fast forward tracking, i.e. any TimedBox received
    /// via input stream START_POS can be tracked towards the current track head
    /// (i.e. last received TrackingData). Measured in number of frames.
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public int StreamingTrackDataCacheSize {
      get { if ((_hasBits0 & 8) != 0) { return streamingTrackDataCacheSize_; } else { return StreamingTrackDataCacheSizeDefaultValue; } }
      set {
        _hasBits0 |= 8;
        streamingTrackDataCacheSize_ = value;
      }
    }
    /// <summary>Gets whether the "streaming_track_data_cache_size" field is set</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool HasStreamingTrackDataCacheSize {
      get { return (_hasBits0 & 8) != 0; }
    }
    /// <summary>Clears the value of the "streaming_track_data_cache_size" field</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void ClearStreamingTrackDataCacheSize() {
      _hasBits0 &= ~8;
    }

    /// <summary>Field number for the "start_pos_transition_frames" field.</summary>
    public const int StartPosTransitionFramesFieldNumber = 7;
    private readonly static int StartPosTransitionFramesDefaultValue = 0;

    private int startPosTransitionFrames_;
    /// <summary>
    /// Add a transition period of N frames to smooth the jump from original
    /// tracking to reset start pos with motion compensation. The transition will
    /// be a linear decay of original tracking result. 0 means no transition.
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public int StartPosTransitionFrames {
      get { if ((_hasBits0 & 16) != 0) { return startPosTransitionFrames_; } else { return StartPosTransitionFramesDefaultValue; } }
      set {
        _hasBits0 |= 16;
        startPosTransitionFrames_ = value;
      }
    }
    /// <summary>Gets whether the "start_pos_transition_frames" field is set</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool HasStartPosTransitionFrames {
      get { return (_hasBits0 & 16) != 0; }
    }
    /// <summary>Clears the value of the "start_pos_transition_frames" field</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void ClearStartPosTransitionFrames() {
      _hasBits0 &= ~16;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override bool Equals(object other) {
      return Equals(other as BoxTrackerCalculatorOptions);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool Equals(BoxTrackerCalculatorOptions other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (!object.Equals(TrackerOptions, other.TrackerOptions)) return false;
      if (!object.Equals(InitialPosition, other.InitialPosition)) return false;
      if (VisualizeTrackingData != other.VisualizeTrackingData) return false;
      if (VisualizeState != other.VisualizeState) return false;
      if (VisualizeInternalState != other.VisualizeInternalState) return false;
      if (StreamingTrackDataCacheSize != other.StreamingTrackDataCacheSize) return false;
      if (StartPosTransitionFrames != other.StartPosTransitionFrames) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override int GetHashCode() {
      int hash = 1;
      if (trackerOptions_ != null) hash ^= TrackerOptions.GetHashCode();
      if (initialPosition_ != null) hash ^= InitialPosition.GetHashCode();
      if (HasVisualizeTrackingData) hash ^= VisualizeTrackingData.GetHashCode();
      if (HasVisualizeState) hash ^= VisualizeState.GetHashCode();
      if (HasVisualizeInternalState) hash ^= VisualizeInternalState.GetHashCode();
      if (HasStreamingTrackDataCacheSize) hash ^= StreamingTrackDataCacheSize.GetHashCode();
      if (HasStartPosTransitionFrames) hash ^= StartPosTransitionFrames.GetHashCode();
      if (_unknownFields != null) {
        hash ^= _unknownFields.GetHashCode();
      }
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void WriteTo(pb::CodedOutputStream output) {
    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      output.WriteRawMessage(this);
    #else
      if (trackerOptions_ != null) {
        output.WriteRawTag(10);
        output.WriteMessage(TrackerOptions);
      }
      if (initialPosition_ != null) {
        output.WriteRawTag(18);
        output.WriteMessage(InitialPosition);
      }
      if (HasVisualizeTrackingData) {
        output.WriteRawTag(24);
        output.WriteBool(VisualizeTrackingData);
      }
      if (HasVisualizeState) {
        output.WriteRawTag(32);
        output.WriteBool(VisualizeState);
      }
      if (HasVisualizeInternalState) {
        output.WriteRawTag(40);
        output.WriteBool(VisualizeInternalState);
      }
      if (HasStreamingTrackDataCacheSize) {
        output.WriteRawTag(48);
        output.WriteInt32(StreamingTrackDataCacheSize);
      }
      if (HasStartPosTransitionFrames) {
        output.WriteRawTag(56);
        output.WriteInt32(StartPosTransitionFrames);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    #endif
    }

    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    void pb::IBufferMessage.InternalWriteTo(ref pb::WriteContext output) {
      if (trackerOptions_ != null) {
        output.WriteRawTag(10);
        output.WriteMessage(TrackerOptions);
      }
      if (initialPosition_ != null) {
        output.WriteRawTag(18);
        output.WriteMessage(InitialPosition);
      }
      if (HasVisualizeTrackingData) {
        output.WriteRawTag(24);
        output.WriteBool(VisualizeTrackingData);
      }
      if (HasVisualizeState) {
        output.WriteRawTag(32);
        output.WriteBool(VisualizeState);
      }
      if (HasVisualizeInternalState) {
        output.WriteRawTag(40);
        output.WriteBool(VisualizeInternalState);
      }
      if (HasStreamingTrackDataCacheSize) {
        output.WriteRawTag(48);
        output.WriteInt32(StreamingTrackDataCacheSize);
      }
      if (HasStartPosTransitionFrames) {
        output.WriteRawTag(56);
        output.WriteInt32(StartPosTransitionFrames);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(ref output);
      }
    }
    #endif

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public int CalculateSize() {
      int size = 0;
      if (trackerOptions_ != null) {
        size += 1 + pb::CodedOutputStream.ComputeMessageSize(TrackerOptions);
      }
      if (initialPosition_ != null) {
        size += 1 + pb::CodedOutputStream.ComputeMessageSize(InitialPosition);
      }
      if (HasVisualizeTrackingData) {
        size += 1 + 1;
      }
      if (HasVisualizeState) {
        size += 1 + 1;
      }
      if (HasVisualizeInternalState) {
        size += 1 + 1;
      }
      if (HasStreamingTrackDataCacheSize) {
        size += 1 + pb::CodedOutputStream.ComputeInt32Size(StreamingTrackDataCacheSize);
      }
      if (HasStartPosTransitionFrames) {
        size += 1 + pb::CodedOutputStream.ComputeInt32Size(StartPosTransitionFrames);
      }
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void MergeFrom(BoxTrackerCalculatorOptions other) {
      if (other == null) {
        return;
      }
      if (other.trackerOptions_ != null) {
        if (trackerOptions_ == null) {
          TrackerOptions = new global::Mediapipe.BoxTrackerOptions();
        }
        TrackerOptions.MergeFrom(other.TrackerOptions);
      }
      if (other.initialPosition_ != null) {
        if (initialPosition_ == null) {
          InitialPosition = new global::Mediapipe.TimedBoxProtoList();
        }
        InitialPosition.MergeFrom(other.InitialPosition);
      }
      if (other.HasVisualizeTrackingData) {
        VisualizeTrackingData = other.VisualizeTrackingData;
      }
      if (other.HasVisualizeState) {
        VisualizeState = other.VisualizeState;
      }
      if (other.HasVisualizeInternalState) {
        VisualizeInternalState = other.VisualizeInternalState;
      }
      if (other.HasStreamingTrackDataCacheSize) {
        StreamingTrackDataCacheSize = other.StreamingTrackDataCacheSize;
      }
      if (other.HasStartPosTransitionFrames) {
        StartPosTransitionFrames = other.StartPosTransitionFrames;
      }
      _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void MergeFrom(pb::CodedInputStream input) {
    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      input.ReadRawMessage(this);
    #else
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, input);
            break;
          case 10: {
            if (trackerOptions_ == null) {
              TrackerOptions = new global::Mediapipe.BoxTrackerOptions();
            }
            input.ReadMessage(TrackerOptions);
            break;
          }
          case 18: {
            if (initialPosition_ == null) {
              InitialPosition = new global::Mediapipe.TimedBoxProtoList();
            }
            input.ReadMessage(InitialPosition);
            break;
          }
          case 24: {
            VisualizeTrackingData = input.ReadBool();
            break;
          }
          case 32: {
            VisualizeState = input.ReadBool();
            break;
          }
          case 40: {
            VisualizeInternalState = input.ReadBool();
            break;
          }
          case 48: {
            StreamingTrackDataCacheSize = input.ReadInt32();
            break;
          }
          case 56: {
            StartPosTransitionFrames = input.ReadInt32();
            break;
          }
        }
      }
    #endif
    }

    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    void pb::IBufferMessage.InternalMergeFrom(ref pb::ParseContext input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, ref input);
            break;
          case 10: {
            if (trackerOptions_ == null) {
              TrackerOptions = new global::Mediapipe.BoxTrackerOptions();
            }
            input.ReadMessage(TrackerOptions);
            break;
          }
          case 18: {
            if (initialPosition_ == null) {
              InitialPosition = new global::Mediapipe.TimedBoxProtoList();
            }
            input.ReadMessage(InitialPosition);
            break;
          }
          case 24: {
            VisualizeTrackingData = input.ReadBool();
            break;
          }
          case 32: {
            VisualizeState = input.ReadBool();
            break;
          }
          case 40: {
            VisualizeInternalState = input.ReadBool();
            break;
          }
          case 48: {
            StreamingTrackDataCacheSize = input.ReadInt32();
            break;
          }
          case 56: {
            StartPosTransitionFrames = input.ReadInt32();
            break;
          }
        }
      }
    }
    #endif

    #region Extensions
    /// <summary>Container for extensions for other messages declared in the BoxTrackerCalculatorOptions message type.</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static partial class Extensions {
      public static readonly pb::Extension<global::Mediapipe.CalculatorOptions, global::Mediapipe.BoxTrackerCalculatorOptions> Ext =
        new pb::Extension<global::Mediapipe.CalculatorOptions, global::Mediapipe.BoxTrackerCalculatorOptions>(268767860, pb::FieldCodec.ForMessage(2150142882, global::Mediapipe.BoxTrackerCalculatorOptions.Parser));
    }
    #endregion

  }

  #endregion

}

#endregion Designer generated code
