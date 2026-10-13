#include "pch-cpp.hpp"





template <typename R, typename T1>
struct VirtualFuncInvoker1
{
	typedef R (*Func)(void*, T1, const RuntimeMethod*);

	static inline R Invoke (Il2CppMethodSlot slot, RuntimeObject* obj, T1 p1)
	{
		const VirtualInvokeData& invokeData = il2cpp_codegen_get_virtual_invoke_data(slot, obj);
		return ((Func)invokeData.methodPtr)(obj, p1, invokeData.method);
	}
};

struct Dictionary_2_tAC32B254416DD510DC3E7E36B0706A6B031D7A53;
struct CharU5BU5D_t799905CF001DD5F13F7DBB310181FC4D8B7D0AAB;
struct IntPtrU5BU5D_tFD177F8C806A6921AD7150264CCC62FA00CAD832;
struct ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918;
struct StackTraceU5BU5D_t32FBCB20930EAF5BAE3F450FF75228E5450DA0DF;
struct TypeU5BU5D_t97234E1129B564EB38B8D85CAC2AD8B5B9522FFB;
struct ArrayPoolEventSource_t1625D61DAE6E94771E6C0003F3AAED90C5CC4070;
struct Assembly_t;
struct Attribute_tFDA8EFEFB0711976D22474794576DAF28F7440AA;
struct Binder_t91BFCE95A7057FADF4D8A1A342AFE52872246235;
struct CultureInfo_t9BA817D41AD55AC8BD07480DD8AC22F8FFA378E0;
struct EventSource_tA86759A1E6F272632C299AAC181C0A67E5C52F25;
struct Hashtable_tEFC3B6496E6747787D8BB761B51F2AE3A8CFFE2D;
struct IDictionary_t6D03155AF1FA9083817AA5B6AD7DEEACC26AB220;
struct IResourceGroveler_tDEE701BD41E9E5D260606F79F75427B42C4CC0C0;
struct MemberFilter_tF644F1AE82F611B677CE1964D5A3277DDA21D553;
struct ResourceManager_t311D6D32A753224008949B32CC6A5468C47498EB;
struct RuntimeAssembly_tA26A4DE82E77826DFC3D58AD976BCFC6BCA918AF;
struct SafeSerializationManager_tCBB85B95DFD1634237140CD892E82D06ECB3F5E6;
struct String_t;
struct Type_t;
struct Version_tE426DB5655D0F22920AE16A2AA9AB7781B8255A7;
struct Void_t4861ACF8F4594C3437BB48B6E56783494B843915;
struct __BlockReflectionAttribute_tF9CCB6C80C243B11D7E246D22DA4EF6537C06570;
struct CultureNameResourceSetPair_t06C5772C09CA853E70E42C0E8BC57FE0AA2CB674;

IL2CPP_EXTERN_C RuntimeClass* ArrayPoolEventSource_t1625D61DAE6E94771E6C0003F3AAED90C5CC4070_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* MissingManifestResourceException_t136A089345909ADB6333D6F4E2AA84C7A00CB3FD_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* ResourceManager_t311D6D32A753224008949B32CC6A5468C47498EB_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* SR_t9075FE1407834F2C979C1BFF7E361F58FF03F131_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C String_t* _stringLiteral727FAFFC692D108CBB9ED57B1C150923409C145B;
IL2CPP_EXTERN_C String_t* _stringLiteral758733BDBED83CBFF4F635AC26CA92AAE477F75D;
IL2CPP_EXTERN_C const RuntimeMethod* ArrayPoolEventSource_BufferAllocated_m572CE08A9E849A2DE898A49D6558A4A8ADDE7538_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* ArrayPoolEventSource_BufferRented_m7C5A25187B594FC905D8DCF2BA968F70AA9007D7_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeType* SR_t4586A97D1DD52F3F2BD3AC6DBDE14E6CA4654C61_0_0_0_var;
struct Exception_t_marshaled_com;
struct Exception_t_marshaled_pinvoke;

struct ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918;

IL2CPP_EXTERN_C_BEGIN
IL2CPP_EXTERN_C_END

#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
struct U3CModuleU3E_t1DDA5A529B0CF73D03FCEF719E0F4BD0E96E8C52 
{
};
struct Attribute_tFDA8EFEFB0711976D22474794576DAF28F7440AA  : public RuntimeObject
{
};
struct EventSource_tA86759A1E6F272632C299AAC181C0A67E5C52F25  : public RuntimeObject
{
	String_t* ___U3CNameU3Ek__BackingField;
};
struct MemberInfo_t  : public RuntimeObject
{
};
struct SR_t4586A97D1DD52F3F2BD3AC6DBDE14E6CA4654C61  : public RuntimeObject
{
};
struct SR_t9075FE1407834F2C979C1BFF7E361F58FF03F131  : public RuntimeObject
{
};
struct String_t  : public RuntimeObject
{
	int32_t ____stringLength;
	Il2CppChar ____firstChar;
};
struct Utilities_t906799109C5504A36C4BFB3E08530C9CB5A07AC4  : public RuntimeObject
{
};
struct ValueType_t6D9B272BD21782F0A9A14F2E41F85A50E97A986F  : public RuntimeObject
{
};
struct ValueType_t6D9B272BD21782F0A9A14F2E41F85A50E97A986F_marshaled_pinvoke
{
};
struct ValueType_t6D9B272BD21782F0A9A14F2E41F85A50E97A986F_marshaled_com
{
};
struct ArrayPoolEventSource_t1625D61DAE6E94771E6C0003F3AAED90C5CC4070  : public EventSource_tA86759A1E6F272632C299AAC181C0A67E5C52F25
{
};
struct Boolean_t09A6377A54BE2F9E6985A8149F19234FD7DDFE22 
{
	bool ___m_value;
};
struct Enum_t2A1A94B24E3B776EEF4E5E485E290BB9D4D072E2  : public ValueType_t6D9B272BD21782F0A9A14F2E41F85A50E97A986F
{
};
struct Enum_t2A1A94B24E3B776EEF4E5E485E290BB9D4D072E2_marshaled_pinvoke
{
};
struct Enum_t2A1A94B24E3B776EEF4E5E485E290BB9D4D072E2_marshaled_com
{
};
struct Int32_t680FF22E76F6EFAD4375103CBBFFA0421349384C 
{
	int32_t ___m_value;
};
struct IntPtr_t 
{
	void* ___m_value;
};
struct UInt32_t1833D51FFA667B18A5AA4B8D34DE284F8495D29B 
{
	uint32_t ___m_value;
};
struct Void_t4861ACF8F4594C3437BB48B6E56783494B843915 
{
	union
	{
		struct
		{
		};
		uint8_t Void_t4861ACF8F4594C3437BB48B6E56783494B843915__padding[1];
	};
};
struct __BlockReflectionAttribute_tF9CCB6C80C243B11D7E246D22DA4EF6537C06570  : public Attribute_tFDA8EFEFB0711976D22474794576DAF28F7440AA
{
};
struct Exception_t  : public RuntimeObject
{
	String_t* ____className;
	String_t* ____message;
	RuntimeObject* ____data;
	Exception_t* ____innerException;
	String_t* ____helpURL;
	RuntimeObject* ____stackTrace;
	String_t* ____stackTraceString;
	String_t* ____remoteStackTraceString;
	int32_t ____remoteStackIndex;
	RuntimeObject* ____dynamicMethods;
	int32_t ____HResult;
	String_t* ____source;
	SafeSerializationManager_tCBB85B95DFD1634237140CD892E82D06ECB3F5E6* ____safeSerializationManager;
	StackTraceU5BU5D_t32FBCB20930EAF5BAE3F450FF75228E5450DA0DF* ___captured_traces;
	IntPtrU5BU5D_tFD177F8C806A6921AD7150264CCC62FA00CAD832* ___native_trace_ips;
	int32_t ___caught_in_unmanaged;
};
struct Exception_t_marshaled_pinvoke
{
	char* ____className;
	char* ____message;
	RuntimeObject* ____data;
	Exception_t_marshaled_pinvoke* ____innerException;
	char* ____helpURL;
	Il2CppIUnknown* ____stackTrace;
	char* ____stackTraceString;
	char* ____remoteStackTraceString;
	int32_t ____remoteStackIndex;
	Il2CppIUnknown* ____dynamicMethods;
	int32_t ____HResult;
	char* ____source;
	SafeSerializationManager_tCBB85B95DFD1634237140CD892E82D06ECB3F5E6* ____safeSerializationManager;
	StackTraceU5BU5D_t32FBCB20930EAF5BAE3F450FF75228E5450DA0DF* ___captured_traces;
	Il2CppSafeArray* ___native_trace_ips;
	int32_t ___caught_in_unmanaged;
};
struct Exception_t_marshaled_com
{
	Il2CppChar* ____className;
	Il2CppChar* ____message;
	RuntimeObject* ____data;
	Exception_t_marshaled_com* ____innerException;
	Il2CppChar* ____helpURL;
	Il2CppIUnknown* ____stackTrace;
	Il2CppChar* ____stackTraceString;
	Il2CppChar* ____remoteStackTraceString;
	int32_t ____remoteStackIndex;
	Il2CppIUnknown* ____dynamicMethods;
	int32_t ____HResult;
	Il2CppChar* ____source;
	SafeSerializationManager_tCBB85B95DFD1634237140CD892E82D06ECB3F5E6* ____safeSerializationManager;
	StackTraceU5BU5D_t32FBCB20930EAF5BAE3F450FF75228E5450DA0DF* ___captured_traces;
	Il2CppSafeArray* ___native_trace_ips;
	int32_t ___caught_in_unmanaged;
};
struct RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B 
{
	intptr_t ___value;
};
struct StringComparison_tE14A55CCFA001A5AC85D754179BF2888F45CC94D 
{
	int32_t ___value__;
};
struct UltimateResourceFallbackLocation_tFA91547D7BF4CEF1101A7C391ECB7B73EE073AB6 
{
	int32_t ___value__;
};
struct BufferAllocatedReason_tE5A09014B16EBB2BB52A49271C7C92260F0BD2E7 
{
	int32_t ___value__;
};
struct EventData_tEB579D26516CF33445B84ADE75307FA8EAF6CA4D 
{
	intptr_t ___U3CDataPointerU3Ek__BackingField;
	int32_t ___U3CSizeU3Ek__BackingField;
	int32_t ___U3CReservedU3Ek__BackingField;
};
struct ResourceManager_t311D6D32A753224008949B32CC6A5468C47498EB  : public RuntimeObject
{
	String_t* ___BaseNameField;
	Hashtable_tEFC3B6496E6747787D8BB761B51F2AE3A8CFFE2D* ___ResourceSets;
	Dictionary_2_tAC32B254416DD510DC3E7E36B0706A6B031D7A53* ____resourceSets;
	String_t* ___moduleDir;
	Assembly_t* ___MainAssembly;
	Type_t* ____locationInfo;
	Type_t* ____userResourceSet;
	CultureInfo_t9BA817D41AD55AC8BD07480DD8AC22F8FFA378E0* ____neutralResourcesCulture;
	CultureNameResourceSetPair_t06C5772C09CA853E70E42C0E8BC57FE0AA2CB674* ____lastUsedResourceCache;
	bool ____ignoreCase;
	bool ___UseManifest;
	bool ___UseSatelliteAssem;
	int32_t ____fallbackLoc;
	Version_tE426DB5655D0F22920AE16A2AA9AB7781B8255A7* ____satelliteContractVersion;
	bool ____lookedForSatelliteContractVersion;
	Assembly_t* ____callingAssembly;
	RuntimeAssembly_tA26A4DE82E77826DFC3D58AD976BCFC6BCA918AF* ___m_callingAssembly;
	RuntimeObject* ___resourceGroveler;
};
struct SystemException_tCC48D868298F4C0705279823E34B00F4FBDB7295  : public Exception_t
{
};
struct Type_t  : public MemberInfo_t
{
	RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B ____impl;
};
struct MissingManifestResourceException_t136A089345909ADB6333D6F4E2AA84C7A00CB3FD  : public SystemException_tCC48D868298F4C0705279823E34B00F4FBDB7295
{
};
struct SR_t9075FE1407834F2C979C1BFF7E361F58FF03F131_StaticFields
{
	ResourceManager_t311D6D32A753224008949B32CC6A5468C47498EB* ___s_resourceManager;
	Type_t* ___U3CResourceTypeU3Ek__BackingField;
};
struct String_t_StaticFields
{
	String_t* ___Empty;
};
struct ArrayPoolEventSource_t1625D61DAE6E94771E6C0003F3AAED90C5CC4070_StaticFields
{
	ArrayPoolEventSource_t1625D61DAE6E94771E6C0003F3AAED90C5CC4070* ___Log;
};
struct Boolean_t09A6377A54BE2F9E6985A8149F19234FD7DDFE22_StaticFields
{
	String_t* ___TrueString;
	String_t* ___FalseString;
};
struct IntPtr_t_StaticFields
{
	intptr_t ___Zero;
};
struct ResourceManager_t311D6D32A753224008949B32CC6A5468C47498EB_StaticFields
{
	int32_t ___MagicNumber;
	int32_t ___HeaderVersionNumber;
	Type_t* ____minResourceSet;
	String_t* ___ResReaderTypeName;
	String_t* ___ResSetTypeName;
	String_t* ___MscorlibName;
	int32_t ___DEBUG;
};
struct Type_t_StaticFields
{
	Binder_t91BFCE95A7057FADF4D8A1A342AFE52872246235* ___s_defaultBinder;
	Il2CppChar ___Delimiter;
	TypeU5BU5D_t97234E1129B564EB38B8D85CAC2AD8B5B9522FFB* ___EmptyTypes;
	RuntimeObject* ___Missing;
	MemberFilter_tF644F1AE82F611B677CE1964D5A3277DDA21D553* ___FilterAttribute;
	MemberFilter_tF644F1AE82F611B677CE1964D5A3277DDA21D553* ___FilterName;
	MemberFilter_tF644F1AE82F611B677CE1964D5A3277DDA21D553* ___FilterNameIgnoreCase;
};
#ifdef __clang__
#pragma clang diagnostic pop
#endif
struct ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918  : public RuntimeArray
{
	ALIGN_FIELD (8) RuntimeObject* m_Items[1];

	inline RuntimeObject* GetAt(il2cpp_array_size_t index) const
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		return m_Items[index];
	}
	inline RuntimeObject** GetAddressAt(il2cpp_array_size_t index)
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		return m_Items + index;
	}
	inline void SetAt(il2cpp_array_size_t index, RuntimeObject* value)
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		m_Items[index] = value;
		Il2CppCodeGenWriteBarrier((void**)m_Items + index, (void*)value);
	}
	inline RuntimeObject* GetAtUnchecked(il2cpp_array_size_t index) const
	{
		return m_Items[index];
	}
	inline RuntimeObject** GetAddressAtUnchecked(il2cpp_array_size_t index)
	{
		return m_Items + index;
	}
	inline void SetAtUnchecked(il2cpp_array_size_t index, RuntimeObject* value)
	{
		m_Items[index] = value;
		Il2CppCodeGenWriteBarrier((void**)m_Items + index, (void*)value);
	}
};



IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR Type_t* SR_get_ResourceType_m6B97A23401AB89F6FCB3A814D6CDF4E953CC905E_inline (const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_NO_INLINE IL2CPP_METHOD_ATTR void ResourceManager__ctor_mC93D478F43E5089ACC407FDECF067A0F208A3784 (ResourceManager_t311D6D32A753224008949B32CC6A5468C47498EB* __this, Type_t* ___0_resourceSource, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR ResourceManager_t311D6D32A753224008949B32CC6A5468C47498EB* SR_get_ResourceManager_mE56944600E46AE81DF2BCE9251781557B00FC72F (const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool String_Equals_m7BDFC0B951005B9DC2BAED464AFE68FF7E9ACE5A (String_t* __this, String_t* ___0_value, int32_t ___1_comparisonType, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_NO_INLINE IL2CPP_METHOD_ATTR bool SR_UsingResourceKeys_m94C0861D4B4644E723DB307AB7EEEC8C5A7E096A (const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* String_Join_m1DE9E42021535E27E1623C55608AC7EF370AB3D7 (String_t* ___0_separator, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___1_values, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* String_Concat_m9E3155FB84015C823606188F53B47CB44C444991 (String_t* ___0_str0, String_t* ___1_str1, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* String_Format_m918500C1EFB475181349A79989BB79BB36102894 (String_t* ___0_format, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___1_args, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* String_Format_mA8DBB4C2516B9723C5A41E6CB1E2FAF4BBE96DD8 (String_t* ___0_format, RuntimeObject* ___1_arg0, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* String_Format_mFB7DA489BD99F4670881FF50EC017BFB0A5C0987 (String_t* ___0_format, RuntimeObject* ___1_arg0, RuntimeObject* ___2_arg1, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* String_Format_mA0534D6E2AE4D67A6BD8D45B3321323930EB930C (String_t* ___0_format, RuntimeObject* ___1_arg0, RuntimeObject* ___2_arg1, RuntimeObject* ___3_arg2, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* SR_GetResourceString_m1A00632A2A8124275F4F313532EF2FF79067EF9F (String_t* ___0_resourceKey, String_t* ___1_defaultString, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Type_t* Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57 (RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B ___0_handle, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Attribute__ctor_m79ED1BF1EE36D1E417BA89A0D9F91F8AAD8D19E2 (Attribute_tFDA8EFEFB0711976D22474794576DAF28F7440AA* __this, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void EventData_set_Size_m4F0086F2CA3FFE1F1E0B57EF54F1324694BD094F_inline (EventData_tEB579D26516CF33445B84ADE75307FA8EAF6CA4D* __this, int32_t ___0_value, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR intptr_t IntPtr_op_Explicit_mE2CEC14C61FD5E2159A03EA2AD97F5CDC5BB9F4D (void* ___0_value, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void EventData_set_DataPointer_m77D62FE7997CE5DC1F29E7DCDFB3C413AEF93929_inline (EventData_tEB579D26516CF33445B84ADE75307FA8EAF6CA4D* __this, intptr_t ___0_value, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void EventSource_WriteEventCore_m168D78A12127FCD1AF49F42E7E3DB2303035B631 (EventSource_tA86759A1E6F272632C299AAC181C0A67E5C52F25* __this, int32_t ___0_eventId, int32_t ___1_eventDataCount, EventData_tEB579D26516CF33445B84ADE75307FA8EAF6CA4D* ___2_data, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void EventSource_WriteEvent_m29A840C8BA6AEBC203406BC9BBED63067788805F (EventSource_tA86759A1E6F272632C299AAC181C0A67E5C52F25* __this, int32_t ___0_eventId, int32_t ___1_arg1, int32_t ___2_arg2, int32_t ___3_arg3, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void EventSource__ctor_mDF751901186604B47C80B7DCC8DCB624FE5664BC (EventSource_tA86759A1E6F272632C299AAC181C0A67E5C52F25* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void ArrayPoolEventSource__ctor_m17673A9A8E97E108F37D21132565BE4CEF6340A8 (ArrayPoolEventSource_t1625D61DAE6E94771E6C0003F3AAED90C5CC4070* __this, const RuntimeMethod* method) ;
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
#ifdef __clang__
#pragma clang diagnostic pop
#endif
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
#ifdef __clang__
#pragma clang diagnostic pop
#endif
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR ResourceManager_t311D6D32A753224008949B32CC6A5468C47498EB* SR_get_ResourceManager_mE56944600E46AE81DF2BCE9251781557B00FC72F (const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&ResourceManager_t311D6D32A753224008949B32CC6A5468C47498EB_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&SR_t9075FE1407834F2C979C1BFF7E361F58FF03F131_il2cpp_TypeInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	ResourceManager_t311D6D32A753224008949B32CC6A5468C47498EB* G_B2_0 = NULL;
	ResourceManager_t311D6D32A753224008949B32CC6A5468C47498EB* G_B1_0 = NULL;
	{
		il2cpp_codegen_runtime_class_init_inline(SR_t9075FE1407834F2C979C1BFF7E361F58FF03F131_il2cpp_TypeInfo_var);
		ResourceManager_t311D6D32A753224008949B32CC6A5468C47498EB* L_0 = ((SR_t9075FE1407834F2C979C1BFF7E361F58FF03F131_StaticFields*)il2cpp_codegen_static_fields_for(SR_t9075FE1407834F2C979C1BFF7E361F58FF03F131_il2cpp_TypeInfo_var))->___s_resourceManager;
		ResourceManager_t311D6D32A753224008949B32CC6A5468C47498EB* L_1 = L_0;
		if (L_1)
		{
			G_B2_0 = L_1;
			goto IL_0019;
		}
		G_B1_0 = L_1;
	}
	{
		il2cpp_codegen_runtime_class_init_inline(SR_t9075FE1407834F2C979C1BFF7E361F58FF03F131_il2cpp_TypeInfo_var);
		Type_t* L_2;
		L_2 = SR_get_ResourceType_m6B97A23401AB89F6FCB3A814D6CDF4E953CC905E_inline(NULL);
		ResourceManager_t311D6D32A753224008949B32CC6A5468C47498EB* L_3 = (ResourceManager_t311D6D32A753224008949B32CC6A5468C47498EB*)il2cpp_codegen_object_new(ResourceManager_t311D6D32A753224008949B32CC6A5468C47498EB_il2cpp_TypeInfo_var);
		ResourceManager__ctor_mC93D478F43E5089ACC407FDECF067A0F208A3784(L_3, L_2, NULL);
		ResourceManager_t311D6D32A753224008949B32CC6A5468C47498EB* L_4 = L_3;
		((SR_t9075FE1407834F2C979C1BFF7E361F58FF03F131_StaticFields*)il2cpp_codegen_static_fields_for(SR_t9075FE1407834F2C979C1BFF7E361F58FF03F131_il2cpp_TypeInfo_var))->___s_resourceManager = L_4;
		Il2CppCodeGenWriteBarrier((void**)(&((SR_t9075FE1407834F2C979C1BFF7E361F58FF03F131_StaticFields*)il2cpp_codegen_static_fields_for(SR_t9075FE1407834F2C979C1BFF7E361F58FF03F131_il2cpp_TypeInfo_var))->___s_resourceManager), (void*)L_4);
		G_B2_0 = L_4;
	}

IL_0019:
	{
		return G_B2_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_NO_INLINE IL2CPP_METHOD_ATTR bool SR_UsingResourceKeys_m94C0861D4B4644E723DB307AB7EEEC8C5A7E096A (const RuntimeMethod* method) 
{
	{
		return (bool)0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* SR_GetResourceString_m1A00632A2A8124275F4F313532EF2FF79067EF9F (String_t* ___0_resourceKey, String_t* ___1_defaultString, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&SR_t9075FE1407834F2C979C1BFF7E361F58FF03F131_il2cpp_TypeInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	String_t* V_0 = NULL;
	il2cpp::utils::ExceptionSupportStack<RuntimeObject*, 1> __active_exceptions;
	{
		V_0 = (String_t*)NULL;
	}
	try
	{
		il2cpp_codegen_runtime_class_init_inline(SR_t9075FE1407834F2C979C1BFF7E361F58FF03F131_il2cpp_TypeInfo_var);
		ResourceManager_t311D6D32A753224008949B32CC6A5468C47498EB* L_0;
		L_0 = SR_get_ResourceManager_mE56944600E46AE81DF2BCE9251781557B00FC72F(NULL);
		String_t* L_1 = ___0_resourceKey;
		NullCheck(L_0);
		String_t* L_2;
		L_2 = VirtualFuncInvoker1< String_t*, String_t* >::Invoke(7, L_0, L_1);
		V_0 = L_2;
		goto IL_0013;
	}
	catch(Il2CppExceptionWrapper& e)
	{
		if(il2cpp_codegen_class_is_assignable_from (((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&MissingManifestResourceException_t136A089345909ADB6333D6F4E2AA84C7A00CB3FD_il2cpp_TypeInfo_var)), il2cpp_codegen_object_class(e.ex)))
		{
			IL2CPP_PUSH_ACTIVE_EXCEPTION(e.ex);
			goto CATCH_0010;
		}
		throw e;
	}

CATCH_0010:
	{
		MissingManifestResourceException_t136A089345909ADB6333D6F4E2AA84C7A00CB3FD* L_3 = ((MissingManifestResourceException_t136A089345909ADB6333D6F4E2AA84C7A00CB3FD*)IL2CPP_GET_ACTIVE_EXCEPTION(MissingManifestResourceException_t136A089345909ADB6333D6F4E2AA84C7A00CB3FD*));;
		IL2CPP_POP_ACTIVE_EXCEPTION(Exception_t*);
		goto IL_0013;
	}

IL_0013:
	{
		String_t* L_4 = ___1_defaultString;
		if (!L_4)
		{
			goto IL_0022;
		}
	}
	{
		String_t* L_5 = ___0_resourceKey;
		String_t* L_6 = V_0;
		NullCheck(L_5);
		bool L_7;
		L_7 = String_Equals_m7BDFC0B951005B9DC2BAED464AFE68FF7E9ACE5A(L_5, L_6, 4, NULL);
		if (!L_7)
		{
			goto IL_0022;
		}
	}
	{
		String_t* L_8 = ___1_defaultString;
		return L_8;
	}

IL_0022:
	{
		String_t* L_9 = V_0;
		return L_9;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* SR_Format_m748D9F93192B26FAEC8705D20A8A7E4EDDCE7321 (String_t* ___0_resourceFormat, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___1_args, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&SR_t9075FE1407834F2C979C1BFF7E361F58FF03F131_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteral758733BDBED83CBFF4F635AC26CA92AAE477F75D);
		s_Il2CppMethodInitialized = true;
	}
	{
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_0 = ___1_args;
		if (!L_0)
		{
			goto IL_0024;
		}
	}
	{
		il2cpp_codegen_runtime_class_init_inline(SR_t9075FE1407834F2C979C1BFF7E361F58FF03F131_il2cpp_TypeInfo_var);
		bool L_1;
		L_1 = SR_UsingResourceKeys_m94C0861D4B4644E723DB307AB7EEEC8C5A7E096A(NULL);
		if (!L_1)
		{
			goto IL_001c;
		}
	}
	{
		String_t* L_2 = ___0_resourceFormat;
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_3 = ___1_args;
		String_t* L_4;
		L_4 = String_Join_m1DE9E42021535E27E1623C55608AC7EF370AB3D7(_stringLiteral758733BDBED83CBFF4F635AC26CA92AAE477F75D, L_3, NULL);
		String_t* L_5;
		L_5 = String_Concat_m9E3155FB84015C823606188F53B47CB44C444991(L_2, L_4, NULL);
		return L_5;
	}

IL_001c:
	{
		String_t* L_6 = ___0_resourceFormat;
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_7 = ___1_args;
		String_t* L_8;
		L_8 = String_Format_m918500C1EFB475181349A79989BB79BB36102894(L_6, L_7, NULL);
		return L_8;
	}

IL_0024:
	{
		String_t* L_9 = ___0_resourceFormat;
		return L_9;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* SR_Format_m711C787FBE5126319FF250AB9D182B627DCC3B14 (String_t* ___0_resourceFormat, RuntimeObject* ___1_p1, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&SR_t9075FE1407834F2C979C1BFF7E361F58FF03F131_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteral758733BDBED83CBFF4F635AC26CA92AAE477F75D);
		s_Il2CppMethodInitialized = true;
	}
	{
		il2cpp_codegen_runtime_class_init_inline(SR_t9075FE1407834F2C979C1BFF7E361F58FF03F131_il2cpp_TypeInfo_var);
		bool L_0;
		L_0 = SR_UsingResourceKeys_m94C0861D4B4644E723DB307AB7EEEC8C5A7E096A(NULL);
		if (!L_0)
		{
			goto IL_0020;
		}
	}
	{
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_1 = (ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918*)(ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918*)SZArrayNew(ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918_il2cpp_TypeInfo_var, (uint32_t)2);
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_2 = L_1;
		String_t* L_3 = ___0_resourceFormat;
		NullCheck(L_2);
		ArrayElementTypeCheck (L_2, L_3);
		(L_2)->SetAt(static_cast<il2cpp_array_size_t>(0), (RuntimeObject*)L_3);
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_4 = L_2;
		RuntimeObject* L_5 = ___1_p1;
		NullCheck(L_4);
		ArrayElementTypeCheck (L_4, L_5);
		(L_4)->SetAt(static_cast<il2cpp_array_size_t>(1), (RuntimeObject*)L_5);
		String_t* L_6;
		L_6 = String_Join_m1DE9E42021535E27E1623C55608AC7EF370AB3D7(_stringLiteral758733BDBED83CBFF4F635AC26CA92AAE477F75D, L_4, NULL);
		return L_6;
	}

IL_0020:
	{
		String_t* L_7 = ___0_resourceFormat;
		RuntimeObject* L_8 = ___1_p1;
		String_t* L_9;
		L_9 = String_Format_mA8DBB4C2516B9723C5A41E6CB1E2FAF4BBE96DD8(L_7, L_8, NULL);
		return L_9;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* SR_Format_m5433E0B25E3EF3E3EC2F83AB8394D377FB497086 (String_t* ___0_resourceFormat, RuntimeObject* ___1_p1, RuntimeObject* ___2_p2, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&SR_t9075FE1407834F2C979C1BFF7E361F58FF03F131_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteral758733BDBED83CBFF4F635AC26CA92AAE477F75D);
		s_Il2CppMethodInitialized = true;
	}
	{
		il2cpp_codegen_runtime_class_init_inline(SR_t9075FE1407834F2C979C1BFF7E361F58FF03F131_il2cpp_TypeInfo_var);
		bool L_0;
		L_0 = SR_UsingResourceKeys_m94C0861D4B4644E723DB307AB7EEEC8C5A7E096A(NULL);
		if (!L_0)
		{
			goto IL_0024;
		}
	}
	{
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_1 = (ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918*)(ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918*)SZArrayNew(ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918_il2cpp_TypeInfo_var, (uint32_t)3);
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_2 = L_1;
		String_t* L_3 = ___0_resourceFormat;
		NullCheck(L_2);
		ArrayElementTypeCheck (L_2, L_3);
		(L_2)->SetAt(static_cast<il2cpp_array_size_t>(0), (RuntimeObject*)L_3);
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_4 = L_2;
		RuntimeObject* L_5 = ___1_p1;
		NullCheck(L_4);
		ArrayElementTypeCheck (L_4, L_5);
		(L_4)->SetAt(static_cast<il2cpp_array_size_t>(1), (RuntimeObject*)L_5);
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_6 = L_4;
		RuntimeObject* L_7 = ___2_p2;
		NullCheck(L_6);
		ArrayElementTypeCheck (L_6, L_7);
		(L_6)->SetAt(static_cast<il2cpp_array_size_t>(2), (RuntimeObject*)L_7);
		String_t* L_8;
		L_8 = String_Join_m1DE9E42021535E27E1623C55608AC7EF370AB3D7(_stringLiteral758733BDBED83CBFF4F635AC26CA92AAE477F75D, L_6, NULL);
		return L_8;
	}

IL_0024:
	{
		String_t* L_9 = ___0_resourceFormat;
		RuntimeObject* L_10 = ___1_p1;
		RuntimeObject* L_11 = ___2_p2;
		String_t* L_12;
		L_12 = String_Format_mFB7DA489BD99F4670881FF50EC017BFB0A5C0987(L_9, L_10, L_11, NULL);
		return L_12;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* SR_Format_m4E1E570F7E0FBB8CD6294EB22D670466D349445F (String_t* ___0_resourceFormat, RuntimeObject* ___1_p1, RuntimeObject* ___2_p2, RuntimeObject* ___3_p3, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&SR_t9075FE1407834F2C979C1BFF7E361F58FF03F131_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteral758733BDBED83CBFF4F635AC26CA92AAE477F75D);
		s_Il2CppMethodInitialized = true;
	}
	{
		il2cpp_codegen_runtime_class_init_inline(SR_t9075FE1407834F2C979C1BFF7E361F58FF03F131_il2cpp_TypeInfo_var);
		bool L_0;
		L_0 = SR_UsingResourceKeys_m94C0861D4B4644E723DB307AB7EEEC8C5A7E096A(NULL);
		if (!L_0)
		{
			goto IL_0028;
		}
	}
	{
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_1 = (ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918*)(ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918*)SZArrayNew(ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918_il2cpp_TypeInfo_var, (uint32_t)4);
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_2 = L_1;
		String_t* L_3 = ___0_resourceFormat;
		NullCheck(L_2);
		ArrayElementTypeCheck (L_2, L_3);
		(L_2)->SetAt(static_cast<il2cpp_array_size_t>(0), (RuntimeObject*)L_3);
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_4 = L_2;
		RuntimeObject* L_5 = ___1_p1;
		NullCheck(L_4);
		ArrayElementTypeCheck (L_4, L_5);
		(L_4)->SetAt(static_cast<il2cpp_array_size_t>(1), (RuntimeObject*)L_5);
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_6 = L_4;
		RuntimeObject* L_7 = ___2_p2;
		NullCheck(L_6);
		ArrayElementTypeCheck (L_6, L_7);
		(L_6)->SetAt(static_cast<il2cpp_array_size_t>(2), (RuntimeObject*)L_7);
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_8 = L_6;
		RuntimeObject* L_9 = ___3_p3;
		NullCheck(L_8);
		ArrayElementTypeCheck (L_8, L_9);
		(L_8)->SetAt(static_cast<il2cpp_array_size_t>(3), (RuntimeObject*)L_9);
		String_t* L_10;
		L_10 = String_Join_m1DE9E42021535E27E1623C55608AC7EF370AB3D7(_stringLiteral758733BDBED83CBFF4F635AC26CA92AAE477F75D, L_8, NULL);
		return L_10;
	}

IL_0028:
	{
		String_t* L_11 = ___0_resourceFormat;
		RuntimeObject* L_12 = ___1_p1;
		RuntimeObject* L_13 = ___2_p2;
		RuntimeObject* L_14 = ___3_p3;
		String_t* L_15;
		L_15 = String_Format_mA0534D6E2AE4D67A6BD8D45B3321323930EB930C(L_11, L_12, L_13, L_14, NULL);
		return L_15;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Type_t* SR_get_ResourceType_m6B97A23401AB89F6FCB3A814D6CDF4E953CC905E (const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&SR_t9075FE1407834F2C979C1BFF7E361F58FF03F131_il2cpp_TypeInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	{
		il2cpp_codegen_runtime_class_init_inline(SR_t9075FE1407834F2C979C1BFF7E361F58FF03F131_il2cpp_TypeInfo_var);
		Type_t* L_0 = ((SR_t9075FE1407834F2C979C1BFF7E361F58FF03F131_StaticFields*)il2cpp_codegen_static_fields_for(SR_t9075FE1407834F2C979C1BFF7E361F58FF03F131_il2cpp_TypeInfo_var))->___U3CResourceTypeU3Ek__BackingField;
		return L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* SR_get_ArgumentException_BufferNotFromPool_mAD3D8B4018FB7584EFF31C8396E055DD15DDA828 (const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&SR_t9075FE1407834F2C979C1BFF7E361F58FF03F131_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteral727FAFFC692D108CBB9ED57B1C150923409C145B);
		s_Il2CppMethodInitialized = true;
	}
	{
		il2cpp_codegen_runtime_class_init_inline(SR_t9075FE1407834F2C979C1BFF7E361F58FF03F131_il2cpp_TypeInfo_var);
		String_t* L_0;
		L_0 = SR_GetResourceString_m1A00632A2A8124275F4F313532EF2FF79067EF9F(_stringLiteral727FAFFC692D108CBB9ED57B1C150923409C145B, (String_t*)NULL, NULL);
		return L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void SR__cctor_mC976FB015F9E12150FFBA5D0F2E00417C21D414B (const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&SR_t4586A97D1DD52F3F2BD3AC6DBDE14E6CA4654C61_0_0_0_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&SR_t9075FE1407834F2C979C1BFF7E361F58FF03F131_il2cpp_TypeInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	{
		RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_0 = { reinterpret_cast<intptr_t> (SR_t4586A97D1DD52F3F2BD3AC6DBDE14E6CA4654C61_0_0_0_var) };
		il2cpp_codegen_runtime_class_init_inline(il2cpp_defaults.systemtype_class);
		Type_t* L_1;
		L_1 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_0, NULL);
		((SR_t9075FE1407834F2C979C1BFF7E361F58FF03F131_StaticFields*)il2cpp_codegen_static_fields_for(SR_t9075FE1407834F2C979C1BFF7E361F58FF03F131_il2cpp_TypeInfo_var))->___U3CResourceTypeU3Ek__BackingField = L_1;
		Il2CppCodeGenWriteBarrier((void**)(&((SR_t9075FE1407834F2C979C1BFF7E361F58FF03F131_StaticFields*)il2cpp_codegen_static_fields_for(SR_t9075FE1407834F2C979C1BFF7E361F58FF03F131_il2cpp_TypeInfo_var))->___U3CResourceTypeU3Ek__BackingField), (void*)L_1);
		return;
	}
}
#ifdef __clang__
#pragma clang diagnostic pop
#endif
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void __BlockReflectionAttribute__ctor_m20F196474A2E4CB29D2A3EC895CED183B080E9CD (__BlockReflectionAttribute_tF9CCB6C80C243B11D7E246D22DA4EF6537C06570* __this, const RuntimeMethod* method) 
{
	{
		Attribute__ctor_m79ED1BF1EE36D1E417BA89A0D9F91F8AAD8D19E2(__this, NULL);
		return;
	}
}
#ifdef __clang__
#pragma clang diagnostic pop
#endif
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void ArrayPoolEventSource_BufferRented_m7C5A25187B594FC905D8DCF2BA968F70AA9007D7 (ArrayPoolEventSource_t1625D61DAE6E94771E6C0003F3AAED90C5CC4070* __this, int32_t ___0_bufferId, int32_t ___1_bufferSize, int32_t ___2_poolId, int32_t ___3_bucketId, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&ArrayPoolEventSource_BufferRented_m7C5A25187B594FC905D8DCF2BA968F70AA9007D7_RuntimeMethod_var);
		s_Il2CppMethodInitialized = true;
	}
	EventData_tEB579D26516CF33445B84ADE75307FA8EAF6CA4D* V_0 = NULL;
	EventData_tEB579D26516CF33445B84ADE75307FA8EAF6CA4D V_1;
	memset((&V_1), 0, sizeof(V_1));
	{
		uint32_t L_0 = sizeof(EventData_tEB579D26516CF33445B84ADE75307FA8EAF6CA4D);
		if ((uintptr_t)((uintptr_t)4) * (uintptr_t)L_0 > (uintptr_t)kIl2CppUIntPtrMax)
			IL2CPP_RAISE_MANAGED_EXCEPTION(il2cpp_codegen_get_overflow_exception(), ArrayPoolEventSource_BufferRented_m7C5A25187B594FC905D8DCF2BA968F70AA9007D7_RuntimeMethod_var);
		intptr_t L_1 = ((intptr_t)il2cpp_codegen_multiply((intptr_t)((uintptr_t)4), (int32_t)L_0));
		int8_t* L_2 = (int8_t*) (L_1 ? alloca(L_1) : NULL);
		memset(L_2, 0, L_1);
		V_0 = (EventData_tEB579D26516CF33445B84ADE75307FA8EAF6CA4D*)(L_2);
		EventData_tEB579D26516CF33445B84ADE75307FA8EAF6CA4D* L_3 = V_0;
		il2cpp_codegen_initobj((&V_1), sizeof(EventData_tEB579D26516CF33445B84ADE75307FA8EAF6CA4D));
		EventData_set_Size_m4F0086F2CA3FFE1F1E0B57EF54F1324694BD094F_inline((&V_1), 4, NULL);
		intptr_t L_4;
		L_4 = IntPtr_op_Explicit_mE2CEC14C61FD5E2159A03EA2AD97F5CDC5BB9F4D((void*)((uintptr_t)(&___0_bufferId)), NULL);
		EventData_set_DataPointer_m77D62FE7997CE5DC1F29E7DCDFB3C413AEF93929_inline((&V_1), L_4, NULL);
		EventData_tEB579D26516CF33445B84ADE75307FA8EAF6CA4D L_5 = V_1;
		*(EventData_tEB579D26516CF33445B84ADE75307FA8EAF6CA4D*)L_3 = L_5;
		EventData_tEB579D26516CF33445B84ADE75307FA8EAF6CA4D* L_6 = V_0;
		uint32_t L_7 = sizeof(EventData_tEB579D26516CF33445B84ADE75307FA8EAF6CA4D);
		il2cpp_codegen_initobj((&V_1), sizeof(EventData_tEB579D26516CF33445B84ADE75307FA8EAF6CA4D));
		EventData_set_Size_m4F0086F2CA3FFE1F1E0B57EF54F1324694BD094F_inline((&V_1), 4, NULL);
		intptr_t L_8;
		L_8 = IntPtr_op_Explicit_mE2CEC14C61FD5E2159A03EA2AD97F5CDC5BB9F4D((void*)((uintptr_t)(&___1_bufferSize)), NULL);
		EventData_set_DataPointer_m77D62FE7997CE5DC1F29E7DCDFB3C413AEF93929_inline((&V_1), L_8, NULL);
		EventData_tEB579D26516CF33445B84ADE75307FA8EAF6CA4D L_9 = V_1;
		*(EventData_tEB579D26516CF33445B84ADE75307FA8EAF6CA4D*)((EventData_tEB579D26516CF33445B84ADE75307FA8EAF6CA4D*)il2cpp_codegen_add((intptr_t)L_6, (int32_t)L_7)) = L_9;
		EventData_tEB579D26516CF33445B84ADE75307FA8EAF6CA4D* L_10 = V_0;
		uint32_t L_11 = sizeof(EventData_tEB579D26516CF33445B84ADE75307FA8EAF6CA4D);
		il2cpp_codegen_initobj((&V_1), sizeof(EventData_tEB579D26516CF33445B84ADE75307FA8EAF6CA4D));
		EventData_set_Size_m4F0086F2CA3FFE1F1E0B57EF54F1324694BD094F_inline((&V_1), 4, NULL);
		intptr_t L_12;
		L_12 = IntPtr_op_Explicit_mE2CEC14C61FD5E2159A03EA2AD97F5CDC5BB9F4D((void*)((uintptr_t)(&___2_poolId)), NULL);
		EventData_set_DataPointer_m77D62FE7997CE5DC1F29E7DCDFB3C413AEF93929_inline((&V_1), L_12, NULL);
		EventData_tEB579D26516CF33445B84ADE75307FA8EAF6CA4D L_13 = V_1;
		*(EventData_tEB579D26516CF33445B84ADE75307FA8EAF6CA4D*)((EventData_tEB579D26516CF33445B84ADE75307FA8EAF6CA4D*)il2cpp_codegen_add((intptr_t)L_10, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)2), (int32_t)L_11)))) = L_13;
		EventData_tEB579D26516CF33445B84ADE75307FA8EAF6CA4D* L_14 = V_0;
		uint32_t L_15 = sizeof(EventData_tEB579D26516CF33445B84ADE75307FA8EAF6CA4D);
		il2cpp_codegen_initobj((&V_1), sizeof(EventData_tEB579D26516CF33445B84ADE75307FA8EAF6CA4D));
		EventData_set_Size_m4F0086F2CA3FFE1F1E0B57EF54F1324694BD094F_inline((&V_1), 4, NULL);
		intptr_t L_16;
		L_16 = IntPtr_op_Explicit_mE2CEC14C61FD5E2159A03EA2AD97F5CDC5BB9F4D((void*)((uintptr_t)(&___3_bucketId)), NULL);
		EventData_set_DataPointer_m77D62FE7997CE5DC1F29E7DCDFB3C413AEF93929_inline((&V_1), L_16, NULL);
		EventData_tEB579D26516CF33445B84ADE75307FA8EAF6CA4D L_17 = V_1;
		*(EventData_tEB579D26516CF33445B84ADE75307FA8EAF6CA4D*)((EventData_tEB579D26516CF33445B84ADE75307FA8EAF6CA4D*)il2cpp_codegen_add((intptr_t)L_14, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)3), (int32_t)L_15)))) = L_17;
		EventData_tEB579D26516CF33445B84ADE75307FA8EAF6CA4D* L_18 = V_0;
		EventSource_WriteEventCore_m168D78A12127FCD1AF49F42E7E3DB2303035B631(__this, 1, 4, L_18, NULL);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void ArrayPoolEventSource_BufferAllocated_m572CE08A9E849A2DE898A49D6558A4A8ADDE7538 (ArrayPoolEventSource_t1625D61DAE6E94771E6C0003F3AAED90C5CC4070* __this, int32_t ___0_bufferId, int32_t ___1_bufferSize, int32_t ___2_poolId, int32_t ___3_bucketId, int32_t ___4_reason, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&ArrayPoolEventSource_BufferAllocated_m572CE08A9E849A2DE898A49D6558A4A8ADDE7538_RuntimeMethod_var);
		s_Il2CppMethodInitialized = true;
	}
	EventData_tEB579D26516CF33445B84ADE75307FA8EAF6CA4D* V_0 = NULL;
	EventData_tEB579D26516CF33445B84ADE75307FA8EAF6CA4D V_1;
	memset((&V_1), 0, sizeof(V_1));
	{
		uint32_t L_0 = sizeof(EventData_tEB579D26516CF33445B84ADE75307FA8EAF6CA4D);
		if ((uintptr_t)((uintptr_t)5) * (uintptr_t)L_0 > (uintptr_t)kIl2CppUIntPtrMax)
			IL2CPP_RAISE_MANAGED_EXCEPTION(il2cpp_codegen_get_overflow_exception(), ArrayPoolEventSource_BufferAllocated_m572CE08A9E849A2DE898A49D6558A4A8ADDE7538_RuntimeMethod_var);
		intptr_t L_1 = ((intptr_t)il2cpp_codegen_multiply((intptr_t)((uintptr_t)5), (int32_t)L_0));
		int8_t* L_2 = (int8_t*) (L_1 ? alloca(L_1) : NULL);
		memset(L_2, 0, L_1);
		V_0 = (EventData_tEB579D26516CF33445B84ADE75307FA8EAF6CA4D*)(L_2);
		EventData_tEB579D26516CF33445B84ADE75307FA8EAF6CA4D* L_3 = V_0;
		il2cpp_codegen_initobj((&V_1), sizeof(EventData_tEB579D26516CF33445B84ADE75307FA8EAF6CA4D));
		EventData_set_Size_m4F0086F2CA3FFE1F1E0B57EF54F1324694BD094F_inline((&V_1), 4, NULL);
		intptr_t L_4;
		L_4 = IntPtr_op_Explicit_mE2CEC14C61FD5E2159A03EA2AD97F5CDC5BB9F4D((void*)((uintptr_t)(&___0_bufferId)), NULL);
		EventData_set_DataPointer_m77D62FE7997CE5DC1F29E7DCDFB3C413AEF93929_inline((&V_1), L_4, NULL);
		EventData_tEB579D26516CF33445B84ADE75307FA8EAF6CA4D L_5 = V_1;
		*(EventData_tEB579D26516CF33445B84ADE75307FA8EAF6CA4D*)L_3 = L_5;
		EventData_tEB579D26516CF33445B84ADE75307FA8EAF6CA4D* L_6 = V_0;
		uint32_t L_7 = sizeof(EventData_tEB579D26516CF33445B84ADE75307FA8EAF6CA4D);
		il2cpp_codegen_initobj((&V_1), sizeof(EventData_tEB579D26516CF33445B84ADE75307FA8EAF6CA4D));
		EventData_set_Size_m4F0086F2CA3FFE1F1E0B57EF54F1324694BD094F_inline((&V_1), 4, NULL);
		intptr_t L_8;
		L_8 = IntPtr_op_Explicit_mE2CEC14C61FD5E2159A03EA2AD97F5CDC5BB9F4D((void*)((uintptr_t)(&___1_bufferSize)), NULL);
		EventData_set_DataPointer_m77D62FE7997CE5DC1F29E7DCDFB3C413AEF93929_inline((&V_1), L_8, NULL);
		EventData_tEB579D26516CF33445B84ADE75307FA8EAF6CA4D L_9 = V_1;
		*(EventData_tEB579D26516CF33445B84ADE75307FA8EAF6CA4D*)((EventData_tEB579D26516CF33445B84ADE75307FA8EAF6CA4D*)il2cpp_codegen_add((intptr_t)L_6, (int32_t)L_7)) = L_9;
		EventData_tEB579D26516CF33445B84ADE75307FA8EAF6CA4D* L_10 = V_0;
		uint32_t L_11 = sizeof(EventData_tEB579D26516CF33445B84ADE75307FA8EAF6CA4D);
		il2cpp_codegen_initobj((&V_1), sizeof(EventData_tEB579D26516CF33445B84ADE75307FA8EAF6CA4D));
		EventData_set_Size_m4F0086F2CA3FFE1F1E0B57EF54F1324694BD094F_inline((&V_1), 4, NULL);
		intptr_t L_12;
		L_12 = IntPtr_op_Explicit_mE2CEC14C61FD5E2159A03EA2AD97F5CDC5BB9F4D((void*)((uintptr_t)(&___2_poolId)), NULL);
		EventData_set_DataPointer_m77D62FE7997CE5DC1F29E7DCDFB3C413AEF93929_inline((&V_1), L_12, NULL);
		EventData_tEB579D26516CF33445B84ADE75307FA8EAF6CA4D L_13 = V_1;
		*(EventData_tEB579D26516CF33445B84ADE75307FA8EAF6CA4D*)((EventData_tEB579D26516CF33445B84ADE75307FA8EAF6CA4D*)il2cpp_codegen_add((intptr_t)L_10, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)2), (int32_t)L_11)))) = L_13;
		EventData_tEB579D26516CF33445B84ADE75307FA8EAF6CA4D* L_14 = V_0;
		uint32_t L_15 = sizeof(EventData_tEB579D26516CF33445B84ADE75307FA8EAF6CA4D);
		il2cpp_codegen_initobj((&V_1), sizeof(EventData_tEB579D26516CF33445B84ADE75307FA8EAF6CA4D));
		EventData_set_Size_m4F0086F2CA3FFE1F1E0B57EF54F1324694BD094F_inline((&V_1), 4, NULL);
		intptr_t L_16;
		L_16 = IntPtr_op_Explicit_mE2CEC14C61FD5E2159A03EA2AD97F5CDC5BB9F4D((void*)((uintptr_t)(&___3_bucketId)), NULL);
		EventData_set_DataPointer_m77D62FE7997CE5DC1F29E7DCDFB3C413AEF93929_inline((&V_1), L_16, NULL);
		EventData_tEB579D26516CF33445B84ADE75307FA8EAF6CA4D L_17 = V_1;
		*(EventData_tEB579D26516CF33445B84ADE75307FA8EAF6CA4D*)((EventData_tEB579D26516CF33445B84ADE75307FA8EAF6CA4D*)il2cpp_codegen_add((intptr_t)L_14, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)3), (int32_t)L_15)))) = L_17;
		EventData_tEB579D26516CF33445B84ADE75307FA8EAF6CA4D* L_18 = V_0;
		uint32_t L_19 = sizeof(EventData_tEB579D26516CF33445B84ADE75307FA8EAF6CA4D);
		il2cpp_codegen_initobj((&V_1), sizeof(EventData_tEB579D26516CF33445B84ADE75307FA8EAF6CA4D));
		EventData_set_Size_m4F0086F2CA3FFE1F1E0B57EF54F1324694BD094F_inline((&V_1), 4, NULL);
		intptr_t L_20;
		L_20 = IntPtr_op_Explicit_mE2CEC14C61FD5E2159A03EA2AD97F5CDC5BB9F4D((void*)((uintptr_t)(&___4_reason)), NULL);
		EventData_set_DataPointer_m77D62FE7997CE5DC1F29E7DCDFB3C413AEF93929_inline((&V_1), L_20, NULL);
		EventData_tEB579D26516CF33445B84ADE75307FA8EAF6CA4D L_21 = V_1;
		*(EventData_tEB579D26516CF33445B84ADE75307FA8EAF6CA4D*)((EventData_tEB579D26516CF33445B84ADE75307FA8EAF6CA4D*)il2cpp_codegen_add((intptr_t)L_18, ((intptr_t)il2cpp_codegen_multiply(((intptr_t)4), (int32_t)L_19)))) = L_21;
		EventData_tEB579D26516CF33445B84ADE75307FA8EAF6CA4D* L_22 = V_0;
		EventSource_WriteEventCore_m168D78A12127FCD1AF49F42E7E3DB2303035B631(__this, 2, 5, L_22, NULL);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void ArrayPoolEventSource_BufferReturned_mFFE94C411A076471011AF6F43663F04471B94763 (ArrayPoolEventSource_t1625D61DAE6E94771E6C0003F3AAED90C5CC4070* __this, int32_t ___0_bufferId, int32_t ___1_bufferSize, int32_t ___2_poolId, const RuntimeMethod* method) 
{
	{
		int32_t L_0 = ___0_bufferId;
		int32_t L_1 = ___1_bufferSize;
		int32_t L_2 = ___2_poolId;
		EventSource_WriteEvent_m29A840C8BA6AEBC203406BC9BBED63067788805F(__this, 3, L_0, L_1, L_2, NULL);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void ArrayPoolEventSource__ctor_m17673A9A8E97E108F37D21132565BE4CEF6340A8 (ArrayPoolEventSource_t1625D61DAE6E94771E6C0003F3AAED90C5CC4070* __this, const RuntimeMethod* method) 
{
	{
		EventSource__ctor_mDF751901186604B47C80B7DCC8DCB624FE5664BC(__this, NULL);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void ArrayPoolEventSource__cctor_m7CCF00E870A197035D44D0FCBE81F87AA1608E80 (const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&ArrayPoolEventSource_t1625D61DAE6E94771E6C0003F3AAED90C5CC4070_il2cpp_TypeInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	{
		ArrayPoolEventSource_t1625D61DAE6E94771E6C0003F3AAED90C5CC4070* L_0 = (ArrayPoolEventSource_t1625D61DAE6E94771E6C0003F3AAED90C5CC4070*)il2cpp_codegen_object_new(ArrayPoolEventSource_t1625D61DAE6E94771E6C0003F3AAED90C5CC4070_il2cpp_TypeInfo_var);
		ArrayPoolEventSource__ctor_m17673A9A8E97E108F37D21132565BE4CEF6340A8(L_0, NULL);
		((ArrayPoolEventSource_t1625D61DAE6E94771E6C0003F3AAED90C5CC4070_StaticFields*)il2cpp_codegen_static_fields_for(ArrayPoolEventSource_t1625D61DAE6E94771E6C0003F3AAED90C5CC4070_il2cpp_TypeInfo_var))->___Log = L_0;
		Il2CppCodeGenWriteBarrier((void**)(&((ArrayPoolEventSource_t1625D61DAE6E94771E6C0003F3AAED90C5CC4070_StaticFields*)il2cpp_codegen_static_fields_for(ArrayPoolEventSource_t1625D61DAE6E94771E6C0003F3AAED90C5CC4070_il2cpp_TypeInfo_var))->___Log), (void*)L_0);
		return;
	}
}
#ifdef __clang__
#pragma clang diagnostic pop
#endif
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
#ifdef __clang__
#pragma clang diagnostic pop
#endif
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Utilities_SelectBucketIndex_mB3FA5DAE83D021AC2A9B4C7F2097C4B869B4A035 (int32_t ___0_bufferSize, const RuntimeMethod* method) 
{
	uint32_t V_0 = 0;
	int32_t V_1 = 0;
	{
		int32_t L_0 = ___0_bufferSize;
		V_0 = ((int32_t)((uint32_t)((int32_t)il2cpp_codegen_subtract(L_0, 1))>>4));
		V_1 = 0;
		uint32_t L_1 = V_0;
		if ((!(((uint32_t)L_1) > ((uint32_t)((int32_t)65535)))))
		{
			goto IL_0018;
		}
	}
	{
		uint32_t L_2 = V_0;
		V_0 = ((int32_t)((uint32_t)L_2>>((int32_t)16)));
		V_1 = ((int32_t)16);
	}

IL_0018:
	{
		uint32_t L_3 = V_0;
		if ((!(((uint32_t)L_3) > ((uint32_t)((int32_t)255)))))
		{
			goto IL_0028;
		}
	}
	{
		uint32_t L_4 = V_0;
		V_0 = ((int32_t)((uint32_t)L_4>>8));
		int32_t L_5 = V_1;
		V_1 = ((int32_t)il2cpp_codegen_add(L_5, 8));
	}

IL_0028:
	{
		uint32_t L_6 = V_0;
		if ((!(((uint32_t)L_6) > ((uint32_t)((int32_t)15)))))
		{
			goto IL_0035;
		}
	}
	{
		uint32_t L_7 = V_0;
		V_0 = ((int32_t)((uint32_t)L_7>>4));
		int32_t L_8 = V_1;
		V_1 = ((int32_t)il2cpp_codegen_add(L_8, 4));
	}

IL_0035:
	{
		uint32_t L_9 = V_0;
		if ((!(((uint32_t)L_9) > ((uint32_t)3))))
		{
			goto IL_0041;
		}
	}
	{
		uint32_t L_10 = V_0;
		V_0 = ((int32_t)((uint32_t)L_10>>2));
		int32_t L_11 = V_1;
		V_1 = ((int32_t)il2cpp_codegen_add(L_11, 2));
	}

IL_0041:
	{
		uint32_t L_12 = V_0;
		if ((!(((uint32_t)L_12) > ((uint32_t)1))))
		{
			goto IL_004d;
		}
	}
	{
		uint32_t L_13 = V_0;
		V_0 = ((int32_t)((uint32_t)L_13>>1));
		int32_t L_14 = V_1;
		V_1 = ((int32_t)il2cpp_codegen_add(L_14, 1));
	}

IL_004d:
	{
		int32_t L_15 = V_1;
		uint32_t L_16 = V_0;
		return ((int32_t)il2cpp_codegen_add(L_15, (int32_t)L_16));
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Utilities_GetMaxSizeForBucket_mD728B36C102C9A83F47D5C0D44BC76A97EDB6109 (int32_t ___0_binIndex, const RuntimeMethod* method) 
{
	int32_t V_0 = 0;
	{
		int32_t L_0 = ___0_binIndex;
		V_0 = ((int32_t)(((int32_t)16)<<((int32_t)(L_0&((int32_t)31)))));
		int32_t L_1 = V_0;
		return L_1;
	}
}
#ifdef __clang__
#pragma clang diagnostic pop
#endif
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR Type_t* SR_get_ResourceType_m6B97A23401AB89F6FCB3A814D6CDF4E953CC905E_inline (const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&SR_t9075FE1407834F2C979C1BFF7E361F58FF03F131_il2cpp_TypeInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	{
		il2cpp_codegen_runtime_class_init_inline(SR_t9075FE1407834F2C979C1BFF7E361F58FF03F131_il2cpp_TypeInfo_var);
		Type_t* L_0 = ((SR_t9075FE1407834F2C979C1BFF7E361F58FF03F131_StaticFields*)il2cpp_codegen_static_fields_for(SR_t9075FE1407834F2C979C1BFF7E361F58FF03F131_il2cpp_TypeInfo_var))->___U3CResourceTypeU3Ek__BackingField;
		return L_0;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void EventData_set_Size_m4F0086F2CA3FFE1F1E0B57EF54F1324694BD094F_inline (EventData_tEB579D26516CF33445B84ADE75307FA8EAF6CA4D* __this, int32_t ___0_value, const RuntimeMethod* method) 
{
	{
		int32_t L_0 = ___0_value;
		__this->___U3CSizeU3Ek__BackingField = L_0;
		return;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void EventData_set_DataPointer_m77D62FE7997CE5DC1F29E7DCDFB3C413AEF93929_inline (EventData_tEB579D26516CF33445B84ADE75307FA8EAF6CA4D* __this, intptr_t ___0_value, const RuntimeMethod* method) 
{
	{
		intptr_t L_0 = ___0_value;
		__this->___U3CDataPointerU3Ek__BackingField = L_0;
		return;
	}
}
