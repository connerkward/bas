// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: mediapipe/calculators/core/split_vector_calculator.proto
// </auto-generated>
#pragma warning disable 1591, 0612, 3021
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
namespace Mediapipe {

  /// <summary>Holder for reflection information generated from mediapipe/calculators/core/split_vector_calculator.proto</summary>
  public static partial class SplitVectorCalculatorReflection {

    #region Descriptor
    /// <summary>File descriptor for mediapipe/calculators/core/split_vector_calculator.proto</summary>
    public static pbr::FileDescriptor Descriptor {
      get { return descriptor; }
    }
    private static pbr::FileDescriptor descriptor;

    static SplitVectorCalculatorReflection() {
      byte[] descriptorData = global::System.Convert.FromBase64String(
          string.Concat(
            "CjhtZWRpYXBpcGUvY2FsY3VsYXRvcnMvY29yZS9zcGxpdF92ZWN0b3JfY2Fs",
            "Y3VsYXRvci5wcm90bxIJbWVkaWFwaXBlGiRtZWRpYXBpcGUvZnJhbWV3b3Jr",
            "L2NhbGN1bGF0b3IucHJvdG8iIwoFUmFuZ2USDQoFYmVnaW4YASABKAUSCwoD",
            "ZW5kGAIgASgFItQBChxTcGxpdFZlY3RvckNhbGN1bGF0b3JPcHRpb25zEiAK",
            "BnJhbmdlcxgBIAMoCzIQLm1lZGlhcGlwZS5SYW5nZRIbCgxlbGVtZW50X29u",
            "bHkYAiABKAg6BWZhbHNlEh4KD2NvbWJpbmVfb3V0cHV0cxgDIAEoCDoFZmFs",
            "c2UyVQoDZXh0EhwubWVkaWFwaXBlLkNhbGN1bGF0b3JPcHRpb25zGI7t2nsg",
            "ASgLMicubWVkaWFwaXBlLlNwbGl0VmVjdG9yQ2FsY3VsYXRvck9wdGlvbnM="));
      descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
          new pbr::FileDescriptor[] { global::Mediapipe.CalculatorReflection.Descriptor, },
          new pbr::GeneratedClrTypeInfo(null, null, new pbr::GeneratedClrTypeInfo[] {
            new pbr::GeneratedClrTypeInfo(typeof(global::Mediapipe.Range), global::Mediapipe.Range.Parser, new[]{ "Begin", "End" }, null, null, null, null),
            new pbr::GeneratedClrTypeInfo(typeof(global::Mediapipe.SplitVectorCalculatorOptions), global::Mediapipe.SplitVectorCalculatorOptions.Parser, new[]{ "Ranges", "ElementOnly", "CombineOutputs" }, null, null, new pb::Extension[] { global::Mediapipe.SplitVectorCalculatorOptions.Extensions.Ext }, null)
          }));
    }
    #endregion

  }
  #region Messages
  /// <summary>
  /// A Range {begin, end} specifies beginning ane ending indices to splice a
  /// vector. A vector v is spliced to have elements v[begin:(end-1)], i.e., with
  /// begin index inclusive and end index exclusive.
  /// </summary>
  public sealed partial class Range : pb::IMessage<Range>
  #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      , pb::IBufferMessage
  #endif
  {
    private static readonly pb::MessageParser<Range> _parser = new pb::MessageParser<Range>(() => new Range());
    private pb::UnknownFieldSet _unknownFields;
    private int _hasBits0;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pb::MessageParser<Range> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::Mediapipe.SplitVectorCalculatorReflection.Descriptor.MessageTypes[0]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public Range() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public Range(Range other) : this() {
      _hasBits0 = other._hasBits0;
      begin_ = other.begin_;
      end_ = other.end_;
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public Range Clone() {
      return new Range(this);
    }

    /// <summary>Field number for the "begin" field.</summary>
    public const int BeginFieldNumber = 1;
    private readonly static int BeginDefaultValue = 0;

    private int begin_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public int Begin {
      get { if ((_hasBits0 & 1) != 0) { return begin_; } else { return BeginDefaultValue; } }
      set {
        _hasBits0 |= 1;
        begin_ = value;
      }
    }
    /// <summary>Gets whether the "begin" field is set</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool HasBegin {
      get { return (_hasBits0 & 1) != 0; }
    }
    /// <summary>Clears the value of the "begin" field</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void ClearBegin() {
      _hasBits0 &= ~1;
    }

    /// <summary>Field number for the "end" field.</summary>
    public const int EndFieldNumber = 2;
    private readonly static int EndDefaultValue = 0;

    private int end_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public int End {
      get { if ((_hasBits0 & 2) != 0) { return end_; } else { return EndDefaultValue; } }
      set {
        _hasBits0 |= 2;
        end_ = value;
      }
    }
    /// <summary>Gets whether the "end" field is set</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool HasEnd {
      get { return (_hasBits0 & 2) != 0; }
    }
    /// <summary>Clears the value of the "end" field</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void ClearEnd() {
      _hasBits0 &= ~2;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override bool Equals(object other) {
      return Equals(other as Range);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool Equals(Range other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (Begin != other.Begin) return false;
      if (End != other.End) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override int GetHashCode() {
      int hash = 1;
      if (HasBegin) hash ^= Begin.GetHashCode();
      if (HasEnd) hash ^= End.GetHashCode();
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
      if (HasBegin) {
        output.WriteRawTag(8);
        output.WriteInt32(Begin);
      }
      if (HasEnd) {
        output.WriteRawTag(16);
        output.WriteInt32(End);
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
      if (HasBegin) {
        output.WriteRawTag(8);
        output.WriteInt32(Begin);
      }
      if (HasEnd) {
        output.WriteRawTag(16);
        output.WriteInt32(End);
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
      if (HasBegin) {
        size += 1 + pb::CodedOutputStream.ComputeInt32Size(Begin);
      }
      if (HasEnd) {
        size += 1 + pb::CodedOutputStream.ComputeInt32Size(End);
      }
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void MergeFrom(Range other) {
      if (other == null) {
        return;
      }
      if (other.HasBegin) {
        Begin = other.Begin;
      }
      if (other.HasEnd) {
        End = other.End;
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
            Begin = input.ReadInt32();
            break;
          }
          case 16: {
            End = input.ReadInt32();
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
            Begin = input.ReadInt32();
            break;
          }
          case 16: {
            End = input.ReadInt32();
            break;
          }
        }
      }
    }
    #endif

  }

  public sealed partial class SplitVectorCalculatorOptions : pb::IMessage<SplitVectorCalculatorOptions>
  #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      , pb::IBufferMessage
  #endif
  {
    private static readonly pb::MessageParser<SplitVectorCalculatorOptions> _parser = new pb::MessageParser<SplitVectorCalculatorOptions>(() => new SplitVectorCalculatorOptions());
    private pb::UnknownFieldSet _unknownFields;
    private int _hasBits0;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pb::MessageParser<SplitVectorCalculatorOptions> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::Mediapipe.SplitVectorCalculatorReflection.Descriptor.MessageTypes[1]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public SplitVectorCalculatorOptions() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public SplitVectorCalculatorOptions(SplitVectorCalculatorOptions other) : this() {
      _hasBits0 = other._hasBits0;
      ranges_ = other.ranges_.Clone();
      elementOnly_ = other.elementOnly_;
      combineOutputs_ = other.combineOutputs_;
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public SplitVectorCalculatorOptions Clone() {
      return new SplitVectorCalculatorOptions(this);
    }

    /// <summary>Field number for the "ranges" field.</summary>
    public const int RangesFieldNumber = 1;
    private static readonly pb::FieldCodec<global::Mediapipe.Range> _repeated_ranges_codec
        = pb::FieldCodec.ForMessage(10, global::Mediapipe.Range.Parser);
    private readonly pbc::RepeatedField<global::Mediapipe.Range> ranges_ = new pbc::RepeatedField<global::Mediapipe.Range>();
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public pbc::RepeatedField<global::Mediapipe.Range> Ranges {
      get { return ranges_; }
    }

    /// <summary>Field number for the "element_only" field.</summary>
    public const int ElementOnlyFieldNumber = 2;
    private readonly static bool ElementOnlyDefaultValue = false;

    private bool elementOnly_;
    /// <summary>
    /// Specify if single element ranges should be outputted as std::vector&lt;T> or
    /// just element of type T. By default, if a range specifies only one element,
    /// it is outputted as an std::vector&lt;T>.
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool ElementOnly {
      get { if ((_hasBits0 & 1) != 0) { return elementOnly_; } else { return ElementOnlyDefaultValue; } }
      set {
        _hasBits0 |= 1;
        elementOnly_ = value;
      }
    }
    /// <summary>Gets whether the "element_only" field is set</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool HasElementOnly {
      get { return (_hasBits0 & 1) != 0; }
    }
    /// <summary>Clears the value of the "element_only" field</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void ClearElementOnly() {
      _hasBits0 &= ~1;
    }

    /// <summary>Field number for the "combine_outputs" field.</summary>
    public const int CombineOutputsFieldNumber = 3;
    private readonly static bool CombineOutputsDefaultValue = false;

    private bool combineOutputs_;
    /// <summary>
    /// Combines output elements to one vector.
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool CombineOutputs {
      get { if ((_hasBits0 & 2) != 0) { return combineOutputs_; } else { return CombineOutputsDefaultValue; } }
      set {
        _hasBits0 |= 2;
        combineOutputs_ = value;
      }
    }
    /// <summary>Gets whether the "combine_outputs" field is set</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool HasCombineOutputs {
      get { return (_hasBits0 & 2) != 0; }
    }
    /// <summary>Clears the value of the "combine_outputs" field</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void ClearCombineOutputs() {
      _hasBits0 &= ~2;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override bool Equals(object other) {
      return Equals(other as SplitVectorCalculatorOptions);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool Equals(SplitVectorCalculatorOptions other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if(!ranges_.Equals(other.ranges_)) return false;
      if (ElementOnly != other.ElementOnly) return false;
      if (CombineOutputs != other.CombineOutputs) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override int GetHashCode() {
      int hash = 1;
      hash ^= ranges_.GetHashCode();
      if (HasElementOnly) hash ^= ElementOnly.GetHashCode();
      if (HasCombineOutputs) hash ^= CombineOutputs.GetHashCode();
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
      ranges_.WriteTo(output, _repeated_ranges_codec);
      if (HasElementOnly) {
        output.WriteRawTag(16);
        output.WriteBool(ElementOnly);
      }
      if (HasCombineOutputs) {
        output.WriteRawTag(24);
        output.WriteBool(CombineOutputs);
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
      ranges_.WriteTo(ref output, _repeated_ranges_codec);
      if (HasElementOnly) {
        output.WriteRawTag(16);
        output.WriteBool(ElementOnly);
      }
      if (HasCombineOutputs) {
        output.WriteRawTag(24);
        output.WriteBool(CombineOutputs);
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
      size += ranges_.CalculateSize(_repeated_ranges_codec);
      if (HasElementOnly) {
        size += 1 + 1;
      }
      if (HasCombineOutputs) {
        size += 1 + 1;
      }
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void MergeFrom(SplitVectorCalculatorOptions other) {
      if (other == null) {
        return;
      }
      ranges_.Add(other.ranges_);
      if (other.HasElementOnly) {
        ElementOnly = other.ElementOnly;
      }
      if (other.HasCombineOutputs) {
        CombineOutputs = other.CombineOutputs;
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
            ranges_.AddEntriesFrom(input, _repeated_ranges_codec);
            break;
          }
          case 16: {
            ElementOnly = input.ReadBool();
            break;
          }
          case 24: {
            CombineOutputs = input.ReadBool();
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
            ranges_.AddEntriesFrom(ref input, _repeated_ranges_codec);
            break;
          }
          case 16: {
            ElementOnly = input.ReadBool();
            break;
          }
          case 24: {
            CombineOutputs = input.ReadBool();
            break;
          }
        }
      }
    }
    #endif

    #region Extensions
    /// <summary>Container for extensions for other messages declared in the SplitVectorCalculatorOptions message type.</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static partial class Extensions {
      public static readonly pb::Extension<global::Mediapipe.CalculatorOptions, global::Mediapipe.SplitVectorCalculatorOptions> Ext =
        new pb::Extension<global::Mediapipe.CalculatorOptions, global::Mediapipe.SplitVectorCalculatorOptions>(259438222, pb::FieldCodec.ForMessage(2075505778, global::Mediapipe.SplitVectorCalculatorOptions.Parser));
    }
    #endregion

  }

  #endregion

}

#endregion Designer generated code
