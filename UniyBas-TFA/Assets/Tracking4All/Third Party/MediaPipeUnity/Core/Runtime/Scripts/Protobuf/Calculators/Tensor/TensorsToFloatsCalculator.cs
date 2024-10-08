// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: mediapipe/calculators/tensor/tensors_to_floats_calculator.proto
// </auto-generated>
#pragma warning disable 1591, 0612, 3021
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
namespace Mediapipe {

  /// <summary>Holder for reflection information generated from mediapipe/calculators/tensor/tensors_to_floats_calculator.proto</summary>
  public static partial class TensorsToFloatsCalculatorReflection {

    #region Descriptor
    /// <summary>File descriptor for mediapipe/calculators/tensor/tensors_to_floats_calculator.proto</summary>
    public static pbr::FileDescriptor Descriptor {
      get { return descriptor; }
    }
    private static pbr::FileDescriptor descriptor;

    static TensorsToFloatsCalculatorReflection() {
      byte[] descriptorData = global::System.Convert.FromBase64String(
          string.Concat(
            "Cj9tZWRpYXBpcGUvY2FsY3VsYXRvcnMvdGVuc29yL3RlbnNvcnNfdG9fZmxv",
            "YXRzX2NhbGN1bGF0b3IucHJvdG8SCW1lZGlhcGlwZRokbWVkaWFwaXBlL2Zy",
            "YW1ld29yay9jYWxjdWxhdG9yLnByb3RvIvUBCiBUZW5zb3JzVG9GbG9hdHND",
            "YWxjdWxhdG9yT3B0aW9ucxJQCgphY3RpdmF0aW9uGAEgASgOMjYubWVkaWFw",
            "aXBlLlRlbnNvcnNUb0Zsb2F0c0NhbGN1bGF0b3JPcHRpb25zLkFjdGl2YXRp",
            "b246BE5PTkUiIwoKQWN0aXZhdGlvbhIICgROT05FEAASCwoHU0lHTU9JRBAB",
            "MloKA2V4dBIcLm1lZGlhcGlwZS5DYWxjdWxhdG9yT3B0aW9ucxjrwuWjASAB",
            "KAsyKy5tZWRpYXBpcGUuVGVuc29yc1RvRmxvYXRzQ2FsY3VsYXRvck9wdGlv",
            "bnM="));
      descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
          new pbr::FileDescriptor[] { global::Mediapipe.CalculatorReflection.Descriptor, },
          new pbr::GeneratedClrTypeInfo(null, null, new pbr::GeneratedClrTypeInfo[] {
            new pbr::GeneratedClrTypeInfo(typeof(global::Mediapipe.TensorsToFloatsCalculatorOptions), global::Mediapipe.TensorsToFloatsCalculatorOptions.Parser, new[]{ "Activation" }, null, new[]{ typeof(global::Mediapipe.TensorsToFloatsCalculatorOptions.Types.Activation) }, new pb::Extension[] { global::Mediapipe.TensorsToFloatsCalculatorOptions.Extensions.Ext }, null)
          }));
    }
    #endregion

  }
  #region Messages
  public sealed partial class TensorsToFloatsCalculatorOptions : pb::IMessage<TensorsToFloatsCalculatorOptions>
  #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      , pb::IBufferMessage
  #endif
  {
    private static readonly pb::MessageParser<TensorsToFloatsCalculatorOptions> _parser = new pb::MessageParser<TensorsToFloatsCalculatorOptions>(() => new TensorsToFloatsCalculatorOptions());
    private pb::UnknownFieldSet _unknownFields;
    private int _hasBits0;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pb::MessageParser<TensorsToFloatsCalculatorOptions> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::Mediapipe.TensorsToFloatsCalculatorReflection.Descriptor.MessageTypes[0]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public TensorsToFloatsCalculatorOptions() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public TensorsToFloatsCalculatorOptions(TensorsToFloatsCalculatorOptions other) : this() {
      _hasBits0 = other._hasBits0;
      activation_ = other.activation_;
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public TensorsToFloatsCalculatorOptions Clone() {
      return new TensorsToFloatsCalculatorOptions(this);
    }

    /// <summary>Field number for the "activation" field.</summary>
    public const int ActivationFieldNumber = 1;
    private readonly static global::Mediapipe.TensorsToFloatsCalculatorOptions.Types.Activation ActivationDefaultValue = global::Mediapipe.TensorsToFloatsCalculatorOptions.Types.Activation.None;

    private global::Mediapipe.TensorsToFloatsCalculatorOptions.Types.Activation activation_;
    /// <summary>
    /// Apply activation function to the floats.
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public global::Mediapipe.TensorsToFloatsCalculatorOptions.Types.Activation Activation {
      get { if ((_hasBits0 & 1) != 0) { return activation_; } else { return ActivationDefaultValue; } }
      set {
        _hasBits0 |= 1;
        activation_ = value;
      }
    }
    /// <summary>Gets whether the "activation" field is set</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool HasActivation {
      get { return (_hasBits0 & 1) != 0; }
    }
    /// <summary>Clears the value of the "activation" field</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void ClearActivation() {
      _hasBits0 &= ~1;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override bool Equals(object other) {
      return Equals(other as TensorsToFloatsCalculatorOptions);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool Equals(TensorsToFloatsCalculatorOptions other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (Activation != other.Activation) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override int GetHashCode() {
      int hash = 1;
      if (HasActivation) hash ^= Activation.GetHashCode();
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
      if (HasActivation) {
        output.WriteRawTag(8);
        output.WriteEnum((int) Activation);
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
      if (HasActivation) {
        output.WriteRawTag(8);
        output.WriteEnum((int) Activation);
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
      if (HasActivation) {
        size += 1 + pb::CodedOutputStream.ComputeEnumSize((int) Activation);
      }
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void MergeFrom(TensorsToFloatsCalculatorOptions other) {
      if (other == null) {
        return;
      }
      if (other.HasActivation) {
        Activation = other.Activation;
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
          case 8: {
            Activation = (global::Mediapipe.TensorsToFloatsCalculatorOptions.Types.Activation) input.ReadEnum();
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
          case 8: {
            Activation = (global::Mediapipe.TensorsToFloatsCalculatorOptions.Types.Activation) input.ReadEnum();
            break;
          }
        }
      }
    }
    #endif

    #region Nested types
    /// <summary>Container for nested types declared in the TensorsToFloatsCalculatorOptions message type.</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static partial class Types {
      public enum Activation {
        [pbr::OriginalName("NONE")] None = 0,
        [pbr::OriginalName("SIGMOID")] Sigmoid = 1,
      }

    }
    #endregion

    #region Extensions
    /// <summary>Container for extensions for other messages declared in the TensorsToFloatsCalculatorOptions message type.</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static partial class Extensions {
      public static readonly pb::Extension<global::Mediapipe.CalculatorOptions, global::Mediapipe.TensorsToFloatsCalculatorOptions> Ext =
        new pb::Extension<global::Mediapipe.CalculatorOptions, global::Mediapipe.TensorsToFloatsCalculatorOptions>(343499115, pb::FieldCodec.ForMessage(2747992922, global::Mediapipe.TensorsToFloatsCalculatorOptions.Parser));
    }
    #endregion

  }

  #endregion

}

#endregion Designer generated code
