// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: mediapipe/framework/packet_generator.proto
// </auto-generated>
#pragma warning disable 1591, 0612, 3021
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
namespace Mediapipe {

  /// <summary>Holder for reflection information generated from mediapipe/framework/packet_generator.proto</summary>
  public static partial class PacketGeneratorReflection {

    #region Descriptor
    /// <summary>File descriptor for mediapipe/framework/packet_generator.proto</summary>
    public static pbr::FileDescriptor Descriptor {
      get { return descriptor; }
    }
    private static pbr::FileDescriptor descriptor;

    static PacketGeneratorReflection() {
      byte[] descriptorData = global::System.Convert.FromBase64String(
          string.Concat(
            "CiptZWRpYXBpcGUvZnJhbWV3b3JrL3BhY2tldF9nZW5lcmF0b3IucHJvdG8S",
            "CW1lZGlhcGlwZSJEChZQYWNrZXRHZW5lcmF0b3JPcHRpb25zEhoKDG1lcmdl",
            "X2ZpZWxkcxgBIAEoCDoEdHJ1ZSoKCKCcARCAgICAAjoCGAEi0wEKFVBhY2tl",
            "dEdlbmVyYXRvckNvbmZpZxIYChBwYWNrZXRfZ2VuZXJhdG9yGAEgASgJEhkK",
            "EWlucHV0X3NpZGVfcGFja2V0GAIgAygJEhcKDmV4dGVybmFsX2lucHV0GOoH",
            "IAMoCRIaChJvdXRwdXRfc2lkZV9wYWNrZXQYAyADKAkSGAoPZXh0ZXJuYWxf",
            "b3V0cHV0GOsHIAMoCRIyCgdvcHRpb25zGAQgASgLMiEubWVkaWFwaXBlLlBh",
            "Y2tldEdlbmVyYXRvck9wdGlvbnM6AhgBQjIKGmNvbS5nb29nbGUubWVkaWFw",
            "aXBlLnByb3RvQhRQYWNrZXRHZW5lcmF0b3JQcm90bw=="));
      descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
          new pbr::FileDescriptor[] { },
          new pbr::GeneratedClrTypeInfo(null, null, new pbr::GeneratedClrTypeInfo[] {
            new pbr::GeneratedClrTypeInfo(typeof(global::Mediapipe.PacketGeneratorOptions), global::Mediapipe.PacketGeneratorOptions.Parser, new[]{ "MergeFields" }, null, null, null, null),
            new pbr::GeneratedClrTypeInfo(typeof(global::Mediapipe.PacketGeneratorConfig), global::Mediapipe.PacketGeneratorConfig.Parser, new[]{ "PacketGenerator", "InputSidePacket", "ExternalInput", "OutputSidePacket", "ExternalOutput", "Options" }, null, null, null, null)
          }));
    }
    #endregion

  }
  #region Messages
  /// <summary>
  /// Options used by a PacketGenerator.
  /// </summary>
  [global::System.ObsoleteAttribute]
  public sealed partial class PacketGeneratorOptions : pb::IExtendableMessage<PacketGeneratorOptions>
  #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      , pb::IBufferMessage
  #endif
  {
    private static readonly pb::MessageParser<PacketGeneratorOptions> _parser = new pb::MessageParser<PacketGeneratorOptions>(() => new PacketGeneratorOptions());
    private pb::UnknownFieldSet _unknownFields;
    private pb::ExtensionSet<PacketGeneratorOptions> _extensions;
    private pb::ExtensionSet<PacketGeneratorOptions> _Extensions { get { return _extensions; } }
    private int _hasBits0;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pb::MessageParser<PacketGeneratorOptions> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::Mediapipe.PacketGeneratorReflection.Descriptor.MessageTypes[0]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public PacketGeneratorOptions() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public PacketGeneratorOptions(PacketGeneratorOptions other) : this() {
      _hasBits0 = other._hasBits0;
      mergeFields_ = other.mergeFields_;
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
      _extensions = pb::ExtensionSet.Clone(other._extensions);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public PacketGeneratorOptions Clone() {
      return new PacketGeneratorOptions(this);
    }

    /// <summary>Field number for the "merge_fields" field.</summary>
    public const int MergeFieldsFieldNumber = 1;
    private readonly static bool MergeFieldsDefaultValue = true;

    private bool mergeFields_;
    /// <summary>
    /// If true, this proto specifies a subset of field values,
    /// which should override corresponding field values.
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool MergeFields {
      get { if ((_hasBits0 & 1) != 0) { return mergeFields_; } else { return MergeFieldsDefaultValue; } }
      set {
        _hasBits0 |= 1;
        mergeFields_ = value;
      }
    }
    /// <summary>Gets whether the "merge_fields" field is set</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool HasMergeFields {
      get { return (_hasBits0 & 1) != 0; }
    }
    /// <summary>Clears the value of the "merge_fields" field</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void ClearMergeFields() {
      _hasBits0 &= ~1;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override bool Equals(object other) {
      return Equals(other as PacketGeneratorOptions);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool Equals(PacketGeneratorOptions other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (MergeFields != other.MergeFields) return false;
      if (!Equals(_extensions, other._extensions)) {
        return false;
      }
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override int GetHashCode() {
      int hash = 1;
      if (HasMergeFields) hash ^= MergeFields.GetHashCode();
      if (_extensions != null) {
        hash ^= _extensions.GetHashCode();
      }
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
      if (HasMergeFields) {
        output.WriteRawTag(8);
        output.WriteBool(MergeFields);
      }
      if (_extensions != null) {
        _extensions.WriteTo(output);
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
      if (HasMergeFields) {
        output.WriteRawTag(8);
        output.WriteBool(MergeFields);
      }
      if (_extensions != null) {
        _extensions.WriteTo(ref output);
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
      if (HasMergeFields) {
        size += 1 + 1;
      }
      if (_extensions != null) {
        size += _extensions.CalculateSize();
      }
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void MergeFrom(PacketGeneratorOptions other) {
      if (other == null) {
        return;
      }
      if (other.HasMergeFields) {
        MergeFields = other.MergeFields;
      }
      pb::ExtensionSet.MergeFrom(ref _extensions, other._extensions);
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
            if (!pb::ExtensionSet.TryMergeFieldFrom(ref _extensions, input)) {
              _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, input);
            }
            break;
          case 8: {
            MergeFields = input.ReadBool();
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
            if (!pb::ExtensionSet.TryMergeFieldFrom(ref _extensions, ref input)) {
              _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, ref input);
            }
            break;
          case 8: {
            MergeFields = input.ReadBool();
            break;
          }
        }
      }
    }
    #endif

    public TValue GetExtension<TValue>(pb::Extension<PacketGeneratorOptions, TValue> extension) {
      return pb::ExtensionSet.Get(ref _extensions, extension);
    }
    public pbc::RepeatedField<TValue> GetExtension<TValue>(pb::RepeatedExtension<PacketGeneratorOptions, TValue> extension) {
      return pb::ExtensionSet.Get(ref _extensions, extension);
    }
    public pbc::RepeatedField<TValue> GetOrInitializeExtension<TValue>(pb::RepeatedExtension<PacketGeneratorOptions, TValue> extension) {
      return pb::ExtensionSet.GetOrInitialize(ref _extensions, extension);
    }
    public void SetExtension<TValue>(pb::Extension<PacketGeneratorOptions, TValue> extension, TValue value) {
      pb::ExtensionSet.Set(ref _extensions, extension, value);
    }
    public bool HasExtension<TValue>(pb::Extension<PacketGeneratorOptions, TValue> extension) {
      return pb::ExtensionSet.Has(ref _extensions, extension);
    }
    public void ClearExtension<TValue>(pb::Extension<PacketGeneratorOptions, TValue> extension) {
      pb::ExtensionSet.Clear(ref _extensions, extension);
    }
    public void ClearExtension<TValue>(pb::RepeatedExtension<PacketGeneratorOptions, TValue> extension) {
      pb::ExtensionSet.Clear(ref _extensions, extension);
    }

  }

  /// <summary>
  /// The settings specifying a packet generator and how it is connected.
  /// </summary>
  [global::System.ObsoleteAttribute]
  public sealed partial class PacketGeneratorConfig : pb::IMessage<PacketGeneratorConfig>
  #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      , pb::IBufferMessage
  #endif
  {
    private static readonly pb::MessageParser<PacketGeneratorConfig> _parser = new pb::MessageParser<PacketGeneratorConfig>(() => new PacketGeneratorConfig());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pb::MessageParser<PacketGeneratorConfig> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::Mediapipe.PacketGeneratorReflection.Descriptor.MessageTypes[1]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public PacketGeneratorConfig() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public PacketGeneratorConfig(PacketGeneratorConfig other) : this() {
      packetGenerator_ = other.packetGenerator_;
      inputSidePacket_ = other.inputSidePacket_.Clone();
      externalInput_ = other.externalInput_.Clone();
      outputSidePacket_ = other.outputSidePacket_.Clone();
      externalOutput_ = other.externalOutput_.Clone();
      options_ = other.options_ != null ? other.options_.Clone() : null;
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public PacketGeneratorConfig Clone() {
      return new PacketGeneratorConfig(this);
    }

    /// <summary>Field number for the "packet_generator" field.</summary>
    public const int PacketGeneratorFieldNumber = 1;
    private readonly static string PacketGeneratorDefaultValue = "";

    private string packetGenerator_;
    /// <summary>
    /// The name of the registered packet generator class.
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public string PacketGenerator {
      get { return packetGenerator_ ?? PacketGeneratorDefaultValue; }
      set {
        packetGenerator_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }
    /// <summary>Gets whether the "packet_generator" field is set</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool HasPacketGenerator {
      get { return packetGenerator_ != null; }
    }
    /// <summary>Clears the value of the "packet_generator" field</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void ClearPacketGenerator() {
      packetGenerator_ = null;
    }

    /// <summary>Field number for the "input_side_packet" field.</summary>
    public const int InputSidePacketFieldNumber = 2;
    private static readonly pb::FieldCodec<string> _repeated_inputSidePacket_codec
        = pb::FieldCodec.ForString(18);
    private readonly pbc::RepeatedField<string> inputSidePacket_ = new pbc::RepeatedField<string>();
    /// <summary>
    /// The names of the input side packets.  The PacketGenerator can choose
    /// to access its input side packets either by index or by tag.
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public pbc::RepeatedField<string> InputSidePacket {
      get { return inputSidePacket_; }
    }

    /// <summary>Field number for the "external_input" field.</summary>
    public const int ExternalInputFieldNumber = 1002;
    private static readonly pb::FieldCodec<string> _repeated_externalInput_codec
        = pb::FieldCodec.ForString(8018);
    private readonly pbc::RepeatedField<string> externalInput_ = new pbc::RepeatedField<string>();
    /// <summary>
    /// DEPRECATED(mgeorg) The old name for input_side_packet.
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public pbc::RepeatedField<string> ExternalInput {
      get { return externalInput_; }
    }

    /// <summary>Field number for the "output_side_packet" field.</summary>
    public const int OutputSidePacketFieldNumber = 3;
    private static readonly pb::FieldCodec<string> _repeated_outputSidePacket_codec
        = pb::FieldCodec.ForString(26);
    private readonly pbc::RepeatedField<string> outputSidePacket_ = new pbc::RepeatedField<string>();
    /// <summary>
    /// The names of the output side packets that this generator produces.
    /// The PacketGenerator can choose to access its output side packets
    /// either by index or by tag.
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public pbc::RepeatedField<string> OutputSidePacket {
      get { return outputSidePacket_; }
    }

    /// <summary>Field number for the "external_output" field.</summary>
    public const int ExternalOutputFieldNumber = 1003;
    private static readonly pb::FieldCodec<string> _repeated_externalOutput_codec
        = pb::FieldCodec.ForString(8026);
    private readonly pbc::RepeatedField<string> externalOutput_ = new pbc::RepeatedField<string>();
    /// <summary>
    /// DEPRECATED(mgeorg) The old name for output_side_packet.
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public pbc::RepeatedField<string> ExternalOutput {
      get { return externalOutput_; }
    }

    /// <summary>Field number for the "options" field.</summary>
    public const int OptionsFieldNumber = 4;
    private global::Mediapipe.PacketGeneratorOptions options_;
    /// <summary>
    /// The options for the packet generator.
    /// </summary>
    [global::System.ObsoleteAttribute]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public global::Mediapipe.PacketGeneratorOptions Options {
      get { return options_; }
      set {
        options_ = value;
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override bool Equals(object other) {
      return Equals(other as PacketGeneratorConfig);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool Equals(PacketGeneratorConfig other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (PacketGenerator != other.PacketGenerator) return false;
      if(!inputSidePacket_.Equals(other.inputSidePacket_)) return false;
      if(!externalInput_.Equals(other.externalInput_)) return false;
      if(!outputSidePacket_.Equals(other.outputSidePacket_)) return false;
      if(!externalOutput_.Equals(other.externalOutput_)) return false;
      if (!object.Equals(Options, other.Options)) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override int GetHashCode() {
      int hash = 1;
      if (HasPacketGenerator) hash ^= PacketGenerator.GetHashCode();
      hash ^= inputSidePacket_.GetHashCode();
      hash ^= externalInput_.GetHashCode();
      hash ^= outputSidePacket_.GetHashCode();
      hash ^= externalOutput_.GetHashCode();
      if (options_ != null) hash ^= Options.GetHashCode();
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
      if (HasPacketGenerator) {
        output.WriteRawTag(10);
        output.WriteString(PacketGenerator);
      }
      inputSidePacket_.WriteTo(output, _repeated_inputSidePacket_codec);
      outputSidePacket_.WriteTo(output, _repeated_outputSidePacket_codec);
      if (options_ != null) {
        output.WriteRawTag(34);
        output.WriteMessage(Options);
      }
      externalInput_.WriteTo(output, _repeated_externalInput_codec);
      externalOutput_.WriteTo(output, _repeated_externalOutput_codec);
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    #endif
    }

    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    void pb::IBufferMessage.InternalWriteTo(ref pb::WriteContext output) {
      if (HasPacketGenerator) {
        output.WriteRawTag(10);
        output.WriteString(PacketGenerator);
      }
      inputSidePacket_.WriteTo(ref output, _repeated_inputSidePacket_codec);
      outputSidePacket_.WriteTo(ref output, _repeated_outputSidePacket_codec);
      if (options_ != null) {
        output.WriteRawTag(34);
        output.WriteMessage(Options);
      }
      externalInput_.WriteTo(ref output, _repeated_externalInput_codec);
      externalOutput_.WriteTo(ref output, _repeated_externalOutput_codec);
      if (_unknownFields != null) {
        _unknownFields.WriteTo(ref output);
      }
    }
    #endif

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public int CalculateSize() {
      int size = 0;
      if (HasPacketGenerator) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(PacketGenerator);
      }
      size += inputSidePacket_.CalculateSize(_repeated_inputSidePacket_codec);
      size += externalInput_.CalculateSize(_repeated_externalInput_codec);
      size += outputSidePacket_.CalculateSize(_repeated_outputSidePacket_codec);
      size += externalOutput_.CalculateSize(_repeated_externalOutput_codec);
      if (options_ != null) {
        size += 1 + pb::CodedOutputStream.ComputeMessageSize(Options);
      }
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void MergeFrom(PacketGeneratorConfig other) {
      if (other == null) {
        return;
      }
      if (other.HasPacketGenerator) {
        PacketGenerator = other.PacketGenerator;
      }
      inputSidePacket_.Add(other.inputSidePacket_);
      externalInput_.Add(other.externalInput_);
      outputSidePacket_.Add(other.outputSidePacket_);
      externalOutput_.Add(other.externalOutput_);
      if (other.options_ != null) {
        if (options_ == null) {
          Options = new global::Mediapipe.PacketGeneratorOptions();
        }
        Options.MergeFrom(other.Options);
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
            PacketGenerator = input.ReadString();
            break;
          }
          case 18: {
            inputSidePacket_.AddEntriesFrom(input, _repeated_inputSidePacket_codec);
            break;
          }
          case 26: {
            outputSidePacket_.AddEntriesFrom(input, _repeated_outputSidePacket_codec);
            break;
          }
          case 34: {
            if (options_ == null) {
              Options = new global::Mediapipe.PacketGeneratorOptions();
            }
            input.ReadMessage(Options);
            break;
          }
          case 8018: {
            externalInput_.AddEntriesFrom(input, _repeated_externalInput_codec);
            break;
          }
          case 8026: {
            externalOutput_.AddEntriesFrom(input, _repeated_externalOutput_codec);
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
            PacketGenerator = input.ReadString();
            break;
          }
          case 18: {
            inputSidePacket_.AddEntriesFrom(ref input, _repeated_inputSidePacket_codec);
            break;
          }
          case 26: {
            outputSidePacket_.AddEntriesFrom(ref input, _repeated_outputSidePacket_codec);
            break;
          }
          case 34: {
            if (options_ == null) {
              Options = new global::Mediapipe.PacketGeneratorOptions();
            }
            input.ReadMessage(Options);
            break;
          }
          case 8018: {
            externalInput_.AddEntriesFrom(ref input, _repeated_externalInput_codec);
            break;
          }
          case 8026: {
            externalOutput_.AddEntriesFrom(ref input, _repeated_externalOutput_codec);
            break;
          }
        }
      }
    }
    #endif

  }

  #endregion

}

#endregion Designer generated code
