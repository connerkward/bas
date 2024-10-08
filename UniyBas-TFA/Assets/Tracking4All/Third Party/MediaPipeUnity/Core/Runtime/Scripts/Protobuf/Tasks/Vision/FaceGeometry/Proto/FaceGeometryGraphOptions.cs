// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: mediapipe/tasks/cc/vision/face_geometry/proto/face_geometry_graph_options.proto
// </auto-generated>
#pragma warning disable 1591, 0612, 3021
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
namespace Mediapipe.Tasks.Vision.FaceGeometry.Proto {

  /// <summary>Holder for reflection information generated from mediapipe/tasks/cc/vision/face_geometry/proto/face_geometry_graph_options.proto</summary>
  public static partial class FaceGeometryGraphOptionsReflection {

    #region Descriptor
    /// <summary>File descriptor for mediapipe/tasks/cc/vision/face_geometry/proto/face_geometry_graph_options.proto</summary>
    public static pbr::FileDescriptor Descriptor {
      get { return descriptor; }
    }
    private static pbr::FileDescriptor descriptor;

    static FaceGeometryGraphOptionsReflection() {
      byte[] descriptorData = global::System.Convert.FromBase64String(
          string.Concat(
            "Ck9tZWRpYXBpcGUvdGFza3MvY2MvdmlzaW9uL2ZhY2VfZ2VvbWV0cnkvcHJv",
            "dG8vZmFjZV9nZW9tZXRyeV9ncmFwaF9vcHRpb25zLnByb3RvEiptZWRpYXBp",
            "cGUudGFza3MudmlzaW9uLmZhY2VfZ2VvbWV0cnkucHJvdG8aLG1lZGlhcGlw",
            "ZS9mcmFtZXdvcmsvY2FsY3VsYXRvcl9vcHRpb25zLnByb3RvGlZtZWRpYXBp",
            "cGUvdGFza3MvY2MvdmlzaW9uL2ZhY2VfZ2VvbWV0cnkvY2FsY3VsYXRvcnMv",
            "Z2VvbWV0cnlfcGlwZWxpbmVfY2FsY3VsYXRvci5wcm90byL/AQoYRmFjZUdl",
            "b21ldHJ5R3JhcGhPcHRpb25zEm4KGWdlb21ldHJ5X3BpcGVsaW5lX29wdGlv",
            "bnMYASABKAsySy5tZWRpYXBpcGUudGFza3MudmlzaW9uLmZhY2VfZ2VvbWV0",
            "cnkuRmFjZUdlb21ldHJ5UGlwZWxpbmVDYWxjdWxhdG9yT3B0aW9uczJzCgNl",
            "eHQSHC5tZWRpYXBpcGUuQ2FsY3VsYXRvck9wdGlvbnMY8qH19QEgASgLMkQu",
            "bWVkaWFwaXBlLnRhc2tzLnZpc2lvbi5mYWNlX2dlb21ldHJ5LnByb3RvLkZh",
            "Y2VHZW9tZXRyeUdyYXBoT3B0aW9uc0JVCjRjb20uZ29vZ2xlLm1lZGlhcGlw",
            "ZS50YXNrcy52aXNpb24uZmFjZWdlb21ldHJ5LnByb3RvQh1GYWNlR2VvbWV0",
            "cnlHcmFwaE9wdGlvbnNQcm90bw=="));
      descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
          new pbr::FileDescriptor[] { global::Mediapipe.CalculatorOptionsReflection.Descriptor, global::Mediapipe.Tasks.Vision.FaceGeometry.GeometryPipelineCalculatorReflection.Descriptor, },
          new pbr::GeneratedClrTypeInfo(null, null, new pbr::GeneratedClrTypeInfo[] {
            new pbr::GeneratedClrTypeInfo(typeof(global::Mediapipe.Tasks.Vision.FaceGeometry.Proto.FaceGeometryGraphOptions), global::Mediapipe.Tasks.Vision.FaceGeometry.Proto.FaceGeometryGraphOptions.Parser, new[]{ "GeometryPipelineOptions" }, null, null, new pb::Extension[] { global::Mediapipe.Tasks.Vision.FaceGeometry.Proto.FaceGeometryGraphOptions.Extensions.Ext }, null)
          }));
    }
    #endregion

  }
  #region Messages
  public sealed partial class FaceGeometryGraphOptions : pb::IMessage<FaceGeometryGraphOptions>
  #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      , pb::IBufferMessage
  #endif
  {
    private static readonly pb::MessageParser<FaceGeometryGraphOptions> _parser = new pb::MessageParser<FaceGeometryGraphOptions>(() => new FaceGeometryGraphOptions());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pb::MessageParser<FaceGeometryGraphOptions> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::Mediapipe.Tasks.Vision.FaceGeometry.Proto.FaceGeometryGraphOptionsReflection.Descriptor.MessageTypes[0]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public FaceGeometryGraphOptions() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public FaceGeometryGraphOptions(FaceGeometryGraphOptions other) : this() {
      geometryPipelineOptions_ = other.geometryPipelineOptions_ != null ? other.geometryPipelineOptions_.Clone() : null;
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public FaceGeometryGraphOptions Clone() {
      return new FaceGeometryGraphOptions(this);
    }

    /// <summary>Field number for the "geometry_pipeline_options" field.</summary>
    public const int GeometryPipelineOptionsFieldNumber = 1;
    private global::Mediapipe.Tasks.Vision.FaceGeometry.FaceGeometryPipelineCalculatorOptions geometryPipelineOptions_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public global::Mediapipe.Tasks.Vision.FaceGeometry.FaceGeometryPipelineCalculatorOptions GeometryPipelineOptions {
      get { return geometryPipelineOptions_; }
      set {
        geometryPipelineOptions_ = value;
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override bool Equals(object other) {
      return Equals(other as FaceGeometryGraphOptions);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool Equals(FaceGeometryGraphOptions other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (!object.Equals(GeometryPipelineOptions, other.GeometryPipelineOptions)) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override int GetHashCode() {
      int hash = 1;
      if (geometryPipelineOptions_ != null) hash ^= GeometryPipelineOptions.GetHashCode();
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
      if (geometryPipelineOptions_ != null) {
        output.WriteRawTag(10);
        output.WriteMessage(GeometryPipelineOptions);
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
      if (geometryPipelineOptions_ != null) {
        output.WriteRawTag(10);
        output.WriteMessage(GeometryPipelineOptions);
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
      if (geometryPipelineOptions_ != null) {
        size += 1 + pb::CodedOutputStream.ComputeMessageSize(GeometryPipelineOptions);
      }
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void MergeFrom(FaceGeometryGraphOptions other) {
      if (other == null) {
        return;
      }
      if (other.geometryPipelineOptions_ != null) {
        if (geometryPipelineOptions_ == null) {
          GeometryPipelineOptions = new global::Mediapipe.Tasks.Vision.FaceGeometry.FaceGeometryPipelineCalculatorOptions();
        }
        GeometryPipelineOptions.MergeFrom(other.GeometryPipelineOptions);
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
            if (geometryPipelineOptions_ == null) {
              GeometryPipelineOptions = new global::Mediapipe.Tasks.Vision.FaceGeometry.FaceGeometryPipelineCalculatorOptions();
            }
            input.ReadMessage(GeometryPipelineOptions);
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
            if (geometryPipelineOptions_ == null) {
              GeometryPipelineOptions = new global::Mediapipe.Tasks.Vision.FaceGeometry.FaceGeometryPipelineCalculatorOptions();
            }
            input.ReadMessage(GeometryPipelineOptions);
            break;
          }
        }
      }
    }
    #endif

    #region Extensions
    /// <summary>Container for extensions for other messages declared in the FaceGeometryGraphOptions message type.</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static partial class Extensions {
      public static readonly pb::Extension<global::Mediapipe.CalculatorOptions, global::Mediapipe.Tasks.Vision.FaceGeometry.Proto.FaceGeometryGraphOptions> Ext =
        new pb::Extension<global::Mediapipe.CalculatorOptions, global::Mediapipe.Tasks.Vision.FaceGeometry.Proto.FaceGeometryGraphOptions>(515723506, pb::FieldCodec.ForMessage(4125788050, global::Mediapipe.Tasks.Vision.FaceGeometry.Proto.FaceGeometryGraphOptions.Parser));
    }
    #endregion

  }

  #endregion

}

#endregion Designer generated code
