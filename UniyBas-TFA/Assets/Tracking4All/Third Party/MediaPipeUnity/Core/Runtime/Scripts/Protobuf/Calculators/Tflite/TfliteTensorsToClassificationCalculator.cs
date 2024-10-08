// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: mediapipe/calculators/tflite/tflite_tensors_to_classification_calculator.proto
// </auto-generated>
#pragma warning disable 1591, 0612, 3021
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
namespace Mediapipe {

  /// <summary>Holder for reflection information generated from mediapipe/calculators/tflite/tflite_tensors_to_classification_calculator.proto</summary>
  public static partial class TfliteTensorsToClassificationCalculatorReflection {

    #region Descriptor
    /// <summary>File descriptor for mediapipe/calculators/tflite/tflite_tensors_to_classification_calculator.proto</summary>
    public static pbr::FileDescriptor Descriptor {
      get { return descriptor; }
    }
    private static pbr::FileDescriptor descriptor;

    static TfliteTensorsToClassificationCalculatorReflection() {
      byte[] descriptorData = global::System.Convert.FromBase64String(
          string.Concat(
            "Ck5tZWRpYXBpcGUvY2FsY3VsYXRvcnMvdGZsaXRlL3RmbGl0ZV90ZW5zb3Jz",
            "X3RvX2NsYXNzaWZpY2F0aW9uX2NhbGN1bGF0b3IucHJvdG8SCW1lZGlhcGlw",
            "ZRokbWVkaWFwaXBlL2ZyYW1ld29yay9jYWxjdWxhdG9yLnByb3RvIvwBCi5U",
            "ZkxpdGVUZW5zb3JzVG9DbGFzc2lmaWNhdGlvbkNhbGN1bGF0b3JPcHRpb25z",
            "EhsKE21pbl9zY29yZV90aHJlc2hvbGQYASABKAISDQoFdG9wX2sYAiABKAUS",
            "FgoObGFiZWxfbWFwX3BhdGgYAyABKAkSHQoVYmluYXJ5X2NsYXNzaWZpY2F0",
            "aW9uGAQgASgIMmcKA2V4dBIcLm1lZGlhcGlwZS5DYWxjdWxhdG9yT3B0aW9u",
            "cxjn3YN/IAEoCzI5Lm1lZGlhcGlwZS5UZkxpdGVUZW5zb3JzVG9DbGFzc2lm",
            "aWNhdGlvbkNhbGN1bGF0b3JPcHRpb25z"));
      descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
          new pbr::FileDescriptor[] { global::Mediapipe.CalculatorReflection.Descriptor, },
          new pbr::GeneratedClrTypeInfo(null, null, new pbr::GeneratedClrTypeInfo[] {
            new pbr::GeneratedClrTypeInfo(typeof(global::Mediapipe.TfLiteTensorsToClassificationCalculatorOptions), global::Mediapipe.TfLiteTensorsToClassificationCalculatorOptions.Parser, new[]{ "MinScoreThreshold", "TopK", "LabelMapPath", "BinaryClassification" }, null, null, new pb::Extension[] { global::Mediapipe.TfLiteTensorsToClassificationCalculatorOptions.Extensions.Ext }, null)
          }));
    }
    #endregion

  }
  #region Messages
  public sealed partial class TfLiteTensorsToClassificationCalculatorOptions : pb::IMessage<TfLiteTensorsToClassificationCalculatorOptions>
  #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      , pb::IBufferMessage
  #endif
  {
    private static readonly pb::MessageParser<TfLiteTensorsToClassificationCalculatorOptions> _parser = new pb::MessageParser<TfLiteTensorsToClassificationCalculatorOptions>(() => new TfLiteTensorsToClassificationCalculatorOptions());
    private pb::UnknownFieldSet _unknownFields;
    private int _hasBits0;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pb::MessageParser<TfLiteTensorsToClassificationCalculatorOptions> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::Mediapipe.TfliteTensorsToClassificationCalculatorReflection.Descriptor.MessageTypes[0]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public TfLiteTensorsToClassificationCalculatorOptions() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public TfLiteTensorsToClassificationCalculatorOptions(TfLiteTensorsToClassificationCalculatorOptions other) : this() {
      _hasBits0 = other._hasBits0;
      minScoreThreshold_ = other.minScoreThreshold_;
      topK_ = other.topK_;
      labelMapPath_ = other.labelMapPath_;
      binaryClassification_ = other.binaryClassification_;
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public TfLiteTensorsToClassificationCalculatorOptions Clone() {
      return new TfLiteTensorsToClassificationCalculatorOptions(this);
    }

    /// <summary>Field number for the "min_score_threshold" field.</summary>
    public const int MinScoreThresholdFieldNumber = 1;
    private readonly static float MinScoreThresholdDefaultValue = 0F;

    private float minScoreThreshold_;
    /// <summary>
    /// Score threshold for preserving the class.
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public float MinScoreThreshold {
      get { if ((_hasBits0 & 1) != 0) { return minScoreThreshold_; } else { return MinScoreThresholdDefaultValue; } }
      set {
        _hasBits0 |= 1;
        minScoreThreshold_ = value;
      }
    }
    /// <summary>Gets whether the "min_score_threshold" field is set</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool HasMinScoreThreshold {
      get { return (_hasBits0 & 1) != 0; }
    }
    /// <summary>Clears the value of the "min_score_threshold" field</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void ClearMinScoreThreshold() {
      _hasBits0 &= ~1;
    }

    /// <summary>Field number for the "top_k" field.</summary>
    public const int TopKFieldNumber = 2;
    private readonly static int TopKDefaultValue = 0;

    private int topK_;
    /// <summary>
    /// Number of highest scoring labels to output.  If top_k is not positive then
    /// all labels are used.
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public int TopK {
      get { if ((_hasBits0 & 2) != 0) { return topK_; } else { return TopKDefaultValue; } }
      set {
        _hasBits0 |= 2;
        topK_ = value;
      }
    }
    /// <summary>Gets whether the "top_k" field is set</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool HasTopK {
      get { return (_hasBits0 & 2) != 0; }
    }
    /// <summary>Clears the value of the "top_k" field</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void ClearTopK() {
      _hasBits0 &= ~2;
    }

    /// <summary>Field number for the "label_map_path" field.</summary>
    public const int LabelMapPathFieldNumber = 3;
    private readonly static string LabelMapPathDefaultValue = "";

    private string labelMapPath_;
    /// <summary>
    /// Path to a label map file for getting the actual name of class ids.
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public string LabelMapPath {
      get { return labelMapPath_ ?? LabelMapPathDefaultValue; }
      set {
        labelMapPath_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }
    /// <summary>Gets whether the "label_map_path" field is set</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool HasLabelMapPath {
      get { return labelMapPath_ != null; }
    }
    /// <summary>Clears the value of the "label_map_path" field</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void ClearLabelMapPath() {
      labelMapPath_ = null;
    }

    /// <summary>Field number for the "binary_classification" field.</summary>
    public const int BinaryClassificationFieldNumber = 4;
    private readonly static bool BinaryClassificationDefaultValue = false;

    private bool binaryClassification_;
    /// <summary>
    /// Whether the input is a single float for binary classification.
    /// When true, only a single float is expected in the input tensor and the
    /// label map, if provided, is expected to have exactly two labels.
    /// The single score(float) represent the probability of first label, and
    /// 1 - score is the probabilility of the second label.
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool BinaryClassification {
      get { if ((_hasBits0 & 4) != 0) { return binaryClassification_; } else { return BinaryClassificationDefaultValue; } }
      set {
        _hasBits0 |= 4;
        binaryClassification_ = value;
      }
    }
    /// <summary>Gets whether the "binary_classification" field is set</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool HasBinaryClassification {
      get { return (_hasBits0 & 4) != 0; }
    }
    /// <summary>Clears the value of the "binary_classification" field</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void ClearBinaryClassification() {
      _hasBits0 &= ~4;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override bool Equals(object other) {
      return Equals(other as TfLiteTensorsToClassificationCalculatorOptions);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool Equals(TfLiteTensorsToClassificationCalculatorOptions other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (!pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(MinScoreThreshold, other.MinScoreThreshold)) return false;
      if (TopK != other.TopK) return false;
      if (LabelMapPath != other.LabelMapPath) return false;
      if (BinaryClassification != other.BinaryClassification) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override int GetHashCode() {
      int hash = 1;
      if (HasMinScoreThreshold) hash ^= pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(MinScoreThreshold);
      if (HasTopK) hash ^= TopK.GetHashCode();
      if (HasLabelMapPath) hash ^= LabelMapPath.GetHashCode();
      if (HasBinaryClassification) hash ^= BinaryClassification.GetHashCode();
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
      if (HasMinScoreThreshold) {
        output.WriteRawTag(13);
        output.WriteFloat(MinScoreThreshold);
      }
      if (HasTopK) {
        output.WriteRawTag(16);
        output.WriteInt32(TopK);
      }
      if (HasLabelMapPath) {
        output.WriteRawTag(26);
        output.WriteString(LabelMapPath);
      }
      if (HasBinaryClassification) {
        output.WriteRawTag(32);
        output.WriteBool(BinaryClassification);
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
      if (HasMinScoreThreshold) {
        output.WriteRawTag(13);
        output.WriteFloat(MinScoreThreshold);
      }
      if (HasTopK) {
        output.WriteRawTag(16);
        output.WriteInt32(TopK);
      }
      if (HasLabelMapPath) {
        output.WriteRawTag(26);
        output.WriteString(LabelMapPath);
      }
      if (HasBinaryClassification) {
        output.WriteRawTag(32);
        output.WriteBool(BinaryClassification);
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
      if (HasMinScoreThreshold) {
        size += 1 + 4;
      }
      if (HasTopK) {
        size += 1 + pb::CodedOutputStream.ComputeInt32Size(TopK);
      }
      if (HasLabelMapPath) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(LabelMapPath);
      }
      if (HasBinaryClassification) {
        size += 1 + 1;
      }
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void MergeFrom(TfLiteTensorsToClassificationCalculatorOptions other) {
      if (other == null) {
        return;
      }
      if (other.HasMinScoreThreshold) {
        MinScoreThreshold = other.MinScoreThreshold;
      }
      if (other.HasTopK) {
        TopK = other.TopK;
      }
      if (other.HasLabelMapPath) {
        LabelMapPath = other.LabelMapPath;
      }
      if (other.HasBinaryClassification) {
        BinaryClassification = other.BinaryClassification;
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
          case 13: {
            MinScoreThreshold = input.ReadFloat();
            break;
          }
          case 16: {
            TopK = input.ReadInt32();
            break;
          }
          case 26: {
            LabelMapPath = input.ReadString();
            break;
          }
          case 32: {
            BinaryClassification = input.ReadBool();
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
          case 13: {
            MinScoreThreshold = input.ReadFloat();
            break;
          }
          case 16: {
            TopK = input.ReadInt32();
            break;
          }
          case 26: {
            LabelMapPath = input.ReadString();
            break;
          }
          case 32: {
            BinaryClassification = input.ReadBool();
            break;
          }
        }
      }
    }
    #endif

    #region Extensions
    /// <summary>Container for extensions for other messages declared in the TfLiteTensorsToClassificationCalculatorOptions message type.</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static partial class Extensions {
      public static readonly pb::Extension<global::Mediapipe.CalculatorOptions, global::Mediapipe.TfLiteTensorsToClassificationCalculatorOptions> Ext =
        new pb::Extension<global::Mediapipe.CalculatorOptions, global::Mediapipe.TfLiteTensorsToClassificationCalculatorOptions>(266399463, pb::FieldCodec.ForMessage(2131195706, global::Mediapipe.TfLiteTensorsToClassificationCalculatorOptions.Parser));
    }
    #endregion

  }

  #endregion

}

#endregion Designer generated code
