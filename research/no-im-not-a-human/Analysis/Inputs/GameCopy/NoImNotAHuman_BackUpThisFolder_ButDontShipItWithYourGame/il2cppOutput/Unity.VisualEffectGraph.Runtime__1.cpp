#include "pch-cpp.hpp"





template <typename T1>
struct VirtualActionInvoker1
{
	typedef void (*Action)(void*, T1, const RuntimeMethod*);

	static inline void Invoke (Il2CppMethodSlot slot, RuntimeObject* obj, T1 p1)
	{
		const VirtualInvokeData& invokeData = il2cpp_codegen_get_virtual_invoke_data(slot, obj);
		((Action)invokeData.methodPtr)(obj, p1, invokeData.method);
	}
};
template <typename R>
struct VirtualFuncInvoker0
{
	typedef R (*Func)(void*, const RuntimeMethod*);

	static inline R Invoke (Il2CppMethodSlot slot, RuntimeObject* obj)
	{
		const VirtualInvokeData& invokeData = il2cpp_codegen_get_virtual_invoke_data(slot, obj);
		return ((Func)invokeData.methodPtr)(obj, invokeData.method);
	}
};
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

struct Action_1_tBAC06A706FB1F9A81AEF7EDF6202FC51D1E52C3A;
struct Func_2_tB530FD2BAF3A03B6BE3CE2AA56B6440DBFD63F0A;
struct IEnumerable_1_tF95C9E01A913DD50575531C8305932628663D9E9;
struct IEnumerable_1_tA380EC22FD5D261770A33C489E6F1C089AAA0D85;
struct InputProcessor_1_tCC7634AB386F9C6888B89C0FD440251E67C5D8F7;
struct List_1_t2CDCA768E7F493F5EDEBC75AEB200FD621354E35;
struct List_1_t242CDEAEC9C92000DA96982CDB9D592DDE2AADAF;
struct List_1_tA239CB83DE5615F348BB0507E45F490F4F7C9A8D;
struct List_1_t5A6730315AA46B39985DF97B343C4E4ACEF77B42;
struct List_1_t77B94703E05C519A9010DD0614F757F974E1CD8B;
struct List_1_t89B39292AD45371F7FDCB295AAE956D33588BC6E;
struct TweenRunner_1_t830EC096236A3CEC7189DFA6E0B2E74C5C97780B;
struct InputProcessor_1U5BU5D_t2D9B681F9F0B39759619A0D4617FB6B20FB10AC0;
struct CharU5BU5D_t799905CF001DD5F13F7DBB310181FC4D8B7D0AAB;
struct ColorU5BU5D_t612261CF293F6FFC3D80AB52259FF0DC2B2CC389;
struct GameObjectU5BU5D_tFF67550DFCE87096D7A3734EA15B75896B2722CF;
struct ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918;
struct SelectableU5BU5D_t4160E135F02A40F75A63F787D36F31FEC6FE91A9;
struct StringU5BU5D_t7674CD946EC0CE7B3AE0BE70E6EE85F2ECD9F248;
struct TypeU5BU5D_t97234E1129B564EB38B8D85CAC2AD8B5B9522FFB;
struct VFXBinderBaseU5BU5D_t1ECF22FF6432C40931AF7B0DEC149B606A33E247;
struct Vector3U5BU5D_tFF1859CCE176131B909E2044F76443064254679C;
struct AnimationTriggers_tA0DC06F89C5280C6DD972F6F4C8A56D7F4F79074;
struct AxisControl_tD6613A2445A3C2BFA22C77E16CA3201AF72354A7;
struct Binder_t91BFCE95A7057FADF4D8A1A342AFE52872246235;
struct ButtonControl_t85949109B98AAF5B7ADC0285F0EC98A61EC88ECF;
struct CancellationTokenSource_tAAE1E0033BCFC233801F8CB4CED5C852B350CB7B;
struct Component_t39FBE53E5EFCF4409111FB22C15FF73717632EC3;
struct DeltaControl_t63053AF5E0CD02B62F3CDE79821E4A12F72D573B;
struct DoubleControl_t3707A6260424A2F3636669A2BAFF5CB5E91D2486;
struct Dropdown_t54C0BDC1441E058BE37E796F68886671C270EF89;
struct ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A;
struct GameObject_t76FEDD663AB33C991A9C9A23129337651094216F;
struct Graphic_tCBFCA4585A19E2B75465AECFEAC43F4016BF7931;
struct Image_tBC1D03F63BF71132E9A5E472B8742F172A011E7E;
struct InputControl_t74F06B623518F992BF8E38656A5E0857169E3E2E;
struct InputDevice_t8BCF67533E872A75779C24C93D1D7085B72D364B;
struct IntegerControl_tA24544EFF42204852F638FF5147F754962C997AB;
struct Light_t1E68479B7782AF2050FAA02A5DC612FD034F18F3;
struct MemberFilter_tF644F1AE82F611B677CE1964D5A3277DDA21D553;
struct MonoBehaviour_t532A11E69716D348D8AA7F854AFCBFCB8AD17F71;
struct Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C;
struct PropertyAttribute_t5E0CB5A6CDA6E24CBD4FF26DE3B0C29D8BB54BF0;
struct RectTransform_t6C5DA5E41A89E0F488B001E45E58963480E543A5;
struct RenderTexture_tBA90C4C3AD9EECCFDDCC632D97C29FAB80D60D27;
struct Selectable_t3251808068A17B8E92FB33590A4C2FA66D456712;
struct Slider_t87EA570E3D6556CABF57456C2F3873FFD86E652F;
struct SphereCollider_tBA111C542CE97F6873DE742757213D6265C7D275;
struct Sprite_tAFF74BC83CD68037494CB0B4F28CBDF8971CAB99;
struct String_t;
struct Terrain_t7F309492F67238DBFBC4566F47385B2A665CF667;
struct TerrainData_t615A68EAC648066681875D47FC641496D12F2E24;
struct Text_tD60B2346DAA6666BF0D822FF607F0B220C2B9E62;
struct Texture_t791CBB51219779964E0E8A2ED7C1AA5F92A4A700;
struct Texture2D_tE6505BC111DD8A424A9DBE8E05D7D09E11FFFCF4;
struct Toggle_tBF13F3EBA485E06826FD8A38F4B4C1380DF21A1F;
struct ToggleGroup_tF2E6FE7D4B17BDBF82462715CFB57C4FDE0A2A2C;
struct TouchControl_t3A64A795FF61A8B27E5EBD61632F23AFCC725079;
struct TouchPhaseControl_t4652C6E0EBFAFA85A31607769876B02C1C9FCAEC;
struct TouchPressControl_tD5BD68D9FDEAC889591DD7A499864893A43C7CEB;
struct Transform_tB27202C6F4E36D225EE28A13E4D662BF99785DB1;
struct Type_t;
struct VFXBinderAttribute_tB68026C86370BE191A6C1F1DD6002F89B7625E0B;
struct VFXBinderBase_t8BC197F500B3A144DF76E77C48FA239C04CDA19C;
struct VFXEventAttribute_tA764A595356174BA39C782A8A64B47B39FB3F518;
struct VFXLightBinder_t5FDD7FBBDBFDB8A2AFCAEC4B46460F37EE4A672A;
struct VFXMultiplePositionBinder_t1891AAD3420705E4BA559CFF54038E8F6B27947F;
struct VFXPlaneBinder_t52D64377F31144687E6984CB470F8F20DF50AB58;
struct VFXPositionBinder_t4CD7239B0C1EA2DA36502BF5D324FAF39632F8A4;
struct VFXPreviousPositionBinder_t287E40B0B297056DD27888F72D017CED6BC1F524;
struct VFXPropertyBinder_t761F7FEC5308F4D9ABF7F0A5A3FAAAA6FE043C0F;
struct VFXPropertyBindingAttribute_t600577C584AF5B4A36738E462D551C25B663311E;
struct VFXRaycastBinder_tD681421294476C02004DF02A34F958796887ED17;
struct VFXSpaceableBinder_t0C24970B59F1F8E2DA67992F0E90DF951BF91706;
struct VFXSphereBinder_t1EC83273D815BCA9C53CD4F7D03C8EAE35811624;
struct VFXTerrainBinder_tBB926D3EE00EA5123B213E37F559F7DDDBFC173F;
struct VFXTransformBinder_t2658F13D9C6472FD34C1B8276A298E2F71FE042A;
struct VFXUIDropdownBinder_tE9AA1FDC5FA865CD3412BE969A5A73A8ADA598EE;
struct VFXUISliderBinder_t1A3AF1ABC9C5F1E0AE11E306E67AA2711FFF9516;
struct VFXUIToggleBinder_t16637477616EDAD883FC73354821CFF850463E1E;
struct VFXVelocityBinder_t70DB0B43C2865624A6C9127476F12BC5B6EA31FC;
struct Vector2Control_t8D1B4021A1D82671AF916D3C0A476AA94E46A432;
struct VisualEffect_t8CF4F0E13AEBF27224BB1323CC103666EDBBCFB0;
struct VisualEffectAsset_t0CD57FD9980CD68033BD96DA356697F92A69E688;
struct Void_t4861ACF8F4594C3437BB48B6E56783494B843915;
struct DropdownEvent_t8A008B010A742724CFC93576D6976E474BB13059;
struct OptionData_t68DC820D58A3ABBAE844326B15A7F14D48FAE55F;
struct OptionDataList_t53255477D0A9C6980AB48693A520EFBC94DFFB96;
struct SliderEvent_t92A82EF6C62E15AF92B640FE2D960E877E8C6555;
struct ToggleEvent_t88B31268F9D6D1882E4F921B14704FB9F7047F02;
struct U3CU3Ec_t32602C6EC431C90BF3D26B05836A825F00E5B9DD;

IL2CPP_EXTERN_C RuntimeClass* Application_tDB03BE91CDF0ACA614A5E0B67CFB77C44EB19B21_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* Debug_t8394C7EEAECA3689C2C9B9DE9C7166D73596276F_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* List_1_t242CDEAEC9C92000DA96982CDB9D592DDE2AADAF_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* List_1_t5A6730315AA46B39985DF97B343C4E4ACEF77B42_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* List_1_t77B94703E05C519A9010DD0614F757F974E1CD8B_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* Math_tEB65DE7CA8B083C412C969C92981C030865486CE_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* Mathf_tE284D016E3B297B72311AAD9EB8F0E643F6A4682_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* Physics_t1244C2983AEAFA149425AFFC3DF53BC91C18ED56_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* Space_tC1A1B173D794CCED4F69F7B78C6C9D50C69C7FBE_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* Texture2D_tE6505BC111DD8A424A9DBE8E05D7D09E11FFFCF4_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* U3CU3Ec_t32602C6EC431C90BF3D26B05836A825F00E5B9DD_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* VFXVelocityBinder_t70DB0B43C2865624A6C9127476F12BC5B6EA31FC_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C String_t* _stringLiteral05C2E121588AEA79FB5F07594533A82A5DE429D8;
IL2CPP_EXTERN_C String_t* _stringLiteral06AC4F16B217B88D4CC0899CFE5A559F57DAA082;
IL2CPP_EXTERN_C String_t* _stringLiteral07F8CD0D44E027CC15F7D8BB6A818FF1D2CF8B1C;
IL2CPP_EXTERN_C String_t* _stringLiteral0CA784FE421CD456ED98BEAB512E327C4C927A88;
IL2CPP_EXTERN_C String_t* _stringLiteral19B7D722FFCBB1EBCC95DE76FB16F022050F3CC8;
IL2CPP_EXTERN_C String_t* _stringLiteral1FC21730ACDFA66F078DA1F77FE2ABE6B0FCE69B;
IL2CPP_EXTERN_C String_t* _stringLiteral25CE67F682345294AA5AF9F9B11847064BADF8E7;
IL2CPP_EXTERN_C String_t* _stringLiteral25F810822C78319CB71CA9C122BF5E40EB3FE7B6;
IL2CPP_EXTERN_C String_t* _stringLiteral2766B41C285F5432EBE7E2CE285EDA03E17C0ED9;
IL2CPP_EXTERN_C String_t* _stringLiteral2C7F0192185C743E1751BE03A4D1C0537E2258D7;
IL2CPP_EXTERN_C String_t* _stringLiteral302BB8A4174F85F0054649375600275F42150D68;
IL2CPP_EXTERN_C String_t* _stringLiteral4242348F79D89713783C664DBCE2420B6B2900DE;
IL2CPP_EXTERN_C String_t* _stringLiteral4FFE0C0087288350842E96EBFBF26816FD08C339;
IL2CPP_EXTERN_C String_t* _stringLiteral5258D5F30E97A1EA5F294DD45A880DA0FCE2FC03;
IL2CPP_EXTERN_C String_t* _stringLiteral566B8B38210FC4AA12A6D5CE58BF53C19ADCC217;
IL2CPP_EXTERN_C String_t* _stringLiteral570A8C820BD3B01A870C767E1B234057A3ABC60F;
IL2CPP_EXTERN_C String_t* _stringLiteral5BEFD8CC60A79699B5BB00E37BAC5B62D371E174;
IL2CPP_EXTERN_C String_t* _stringLiteral666AF83F7D0247EED6BA0E39B022A08AF49F95C6;
IL2CPP_EXTERN_C String_t* _stringLiteral68E5CEC31382AB192FF06E62F708EC196A878699;
IL2CPP_EXTERN_C String_t* _stringLiteral6978D594BBF5E4D716F6B6F9237F05A81DE62CB4;
IL2CPP_EXTERN_C String_t* _stringLiteral6CD7D939B718E76DDCB166380F92116EA604BA0E;
IL2CPP_EXTERN_C String_t* _stringLiteral74B50038C7C7797E6099EF7EC0273AAC048D793D;
IL2CPP_EXTERN_C String_t* _stringLiteral76784E8E2FFDA985828158A43B730DE1F4684442;
IL2CPP_EXTERN_C String_t* _stringLiteral7D3C8036CD42ED64DAF6DA744496C82A2C81F94C;
IL2CPP_EXTERN_C String_t* _stringLiteral80DFB27CCC6CCF3576955208EDB98ABF86D1E5D8;
IL2CPP_EXTERN_C String_t* _stringLiteral82D26207184485AEC867969F0F1F452A4ADA97A6;
IL2CPP_EXTERN_C String_t* _stringLiteral880249E7973673F42C9C8FC573E4CC1ECC6758F3;
IL2CPP_EXTERN_C String_t* _stringLiteral8C84BD48DC65AFD12F768ECC14BFBB353EC1DA63;
IL2CPP_EXTERN_C String_t* _stringLiteral8E949677B6004F3DD1FFF60DF500D893920D80EA;
IL2CPP_EXTERN_C String_t* _stringLiteral95E03FC0AED08252F1928B593F01D7D66A1AB1EF;
IL2CPP_EXTERN_C String_t* _stringLiteralA7B1476A1E5CD3BB02076DE8E002321B1E3433D5;
IL2CPP_EXTERN_C String_t* _stringLiteralAAA301CFC06E306888A9185B3D14CBEEA6C6A2C4;
IL2CPP_EXTERN_C String_t* _stringLiteralB037A83EE90C5BB4ED7FEF3171A251ADB4E14BE4;
IL2CPP_EXTERN_C String_t* _stringLiteralB0E010F888B7DEEF0B2CD01DD03D8C9A96B9AAFB;
IL2CPP_EXTERN_C String_t* _stringLiteralBCA55B34FA6BC5431A8F9B97D8C7697DFED6E45F;
IL2CPP_EXTERN_C String_t* _stringLiteralBDBB8D317096A0BA6BA9463FC2533707873DF8FD;
IL2CPP_EXTERN_C String_t* _stringLiteralCA4934A377CA96643EB37C34CCF365B989D5F823;
IL2CPP_EXTERN_C String_t* _stringLiteralCA4FE6DE4D088688B7375BECF5966DF1682F104D;
IL2CPP_EXTERN_C String_t* _stringLiteralCD758053CBD26018308E17600F943A6C1A513D7E;
IL2CPP_EXTERN_C String_t* _stringLiteralCDADA8FD6879C24AEC644DDDE1E86A99C7090E2E;
IL2CPP_EXTERN_C String_t* _stringLiteralD0105C238A624817A3CDCE1394508C5E05E41D06;
IL2CPP_EXTERN_C String_t* _stringLiteralD6C58774A915D2F4C64ACFD3A7512D0D305BD387;
IL2CPP_EXTERN_C String_t* _stringLiteralDDD2EA3792482A37AF09C85C0BB5CA7E47820DC9;
IL2CPP_EXTERN_C String_t* _stringLiteralE1126E892A56AB58E8CA367065ED4190ACACBE26;
IL2CPP_EXTERN_C String_t* _stringLiteralE5A22B80916586DE88322B31E1516201F5FE6993;
IL2CPP_EXTERN_C String_t* _stringLiteralE6971243F7EB358A7C6E0D382075D789ADC51C31;
IL2CPP_EXTERN_C String_t* _stringLiteralEC77BC114B027ACF6D87D758555A29750B472EBC;
IL2CPP_EXTERN_C const RuntimeMethod* Array_Empty_TisRuntimeObject_mFB8A63D602BB6974D31E20300D9EB89C6FE7C278_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* Component_GetComponent_TisVFXPropertyBinder_t761F7FEC5308F4D9ABF7F0A5A3FAAAA6FE043C0F_m483C444935A49F5B275955F30616CCD6440760CA_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* Component_GetComponent_TisVisualEffect_t8CF4F0E13AEBF27224BB1323CC103666EDBBCFB0_mC2DD444DE804211C8EADF53E224C7103F9A3CCD1_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* Component_GetComponents_TisVFXBinderBase_t8BC197F500B3A144DF76E77C48FA239C04CDA19C_m53E17DF23C2BBA4AD8DE56C0C006D00288F75A05_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* Enumerator_Dispose_m13D8B7747BCE3651CB2FD43622DBEDC3E50EA0E6_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* Enumerator_MoveNext_m9C3D158CB944A2244D518FA7989D8312ACCD7C8A_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* Enumerator_get_Current_m098C9AE095DEFD00B1B0314A772330F537805EE2_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* GameObject_GetComponents_TisVFXBinderBase_t8BC197F500B3A144DF76E77C48FA239C04CDA19C_m2849BF084DFC48ABA7745E3C7EC2249159F03323_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* List_1_AddRange_mD0480735EA168951277C2C2EB3F55E76699E8795_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* List_1_Add_m4EA3769821DBDED34CA0EBACD784AC6C1A536C1A_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* List_1_Add_m795E062030C3253617A08E869C6D58DF7FEC6799_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* List_1_Add_m79E50C4F592B1703F4B76A8BE7B4855515460CA1_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* List_1_Contains_mEB538ED5525355D6EF6699D85A92560EDA91A11F_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* List_1_GetEnumerator_m24A1CBC9428047F6672EC9B92A3E4154DC60337A_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* List_1_Remove_mB0623D1E0568D9930B5F47623B7220C2F47B6CB1_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* List_1_ToArray_mD1EFBC7A0C9E4DF640DF58EC90D5494500C0571F_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* List_1__ctor_m18A0E45DF3874B841F030036ABD1CBDCF7D77555_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* List_1__ctor_mC54E2BCBE43279A96FC082F5CDE2D76388BD8F9C_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* List_1__ctor_mE1298EFE02E4CF09A9513D04C6D0FE12C4A5A96D_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* List_1_get_Count_m46EEFFA770BE665EA0CB3A5332E941DA4B3C1D37_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* List_1_get_Count_mF8853CB1C215FFD0674D1337D23514D811BBF2B2_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* List_1_get_Item_m33073C80B80A639ECF4F79B6BDBE709573170A4E_RuntimeMethod_var;

struct ColorU5BU5D_t612261CF293F6FFC3D80AB52259FF0DC2B2CC389;
struct GameObjectU5BU5D_tFF67550DFCE87096D7A3734EA15B75896B2722CF;
struct ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918;
struct StringU5BU5D_t7674CD946EC0CE7B3AE0BE70E6EE85F2ECD9F248;
struct VFXBinderBaseU5BU5D_t1ECF22FF6432C40931AF7B0DEC149B606A33E247;
struct Vector3U5BU5D_tFF1859CCE176131B909E2044F76443064254679C;

IL2CPP_EXTERN_C_BEGIN
IL2CPP_EXTERN_C_END

#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
struct EmptyArray_1_tDF0DD7256B115243AA6BD5558417387A734240EE  : public RuntimeObject
{
};
struct List_1_t242CDEAEC9C92000DA96982CDB9D592DDE2AADAF  : public RuntimeObject
{
	ColorU5BU5D_t612261CF293F6FFC3D80AB52259FF0DC2B2CC389* ____items;
	int32_t ____size;
	int32_t ____version;
	RuntimeObject* ____syncRoot;
};
struct List_1_tA239CB83DE5615F348BB0507E45F490F4F7C9A8D  : public RuntimeObject
{
	ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ____items;
	int32_t ____size;
	int32_t ____version;
	RuntimeObject* ____syncRoot;
};
struct List_1_t5A6730315AA46B39985DF97B343C4E4ACEF77B42  : public RuntimeObject
{
	VFXBinderBaseU5BU5D_t1ECF22FF6432C40931AF7B0DEC149B606A33E247* ____items;
	int32_t ____size;
	int32_t ____version;
	RuntimeObject* ____syncRoot;
};
struct List_1_t77B94703E05C519A9010DD0614F757F974E1CD8B  : public RuntimeObject
{
	Vector3U5BU5D_tFF1859CCE176131B909E2044F76443064254679C* ____items;
	int32_t ____size;
	int32_t ____version;
	RuntimeObject* ____syncRoot;
};
struct U3CPrivateImplementationDetailsU3E_tE3CD203A299B6432ED139A111B41EA2D218822C7  : public RuntimeObject
{
};
struct Attribute_tFDA8EFEFB0711976D22474794576DAF28F7440AA  : public RuntimeObject
{
};
struct ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A  : public RuntimeObject
{
	String_t* ___m_Name;
	int32_t ___m_Id;
};
struct MemberInfo_t  : public RuntimeObject
{
};
struct String_t  : public RuntimeObject
{
	int32_t ____stringLength;
	Il2CppChar ____firstChar;
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
struct U3CU3Ec_t32602C6EC431C90BF3D26B05836A825F00E5B9DD  : public RuntimeObject
{
};
struct InlinedArray_1_tD4AEEFFEC2CBF49D196A92D16CE8DA15F1532FCD 
{
	int32_t ___length;
	InputProcessor_1_tCC7634AB386F9C6888B89C0FD440251E67C5D8F7* ___firstValue;
	InputProcessor_1U5BU5D_t2D9B681F9F0B39759619A0D4617FB6B20FB10AC0* ___additionalValues;
};
struct Boolean_t09A6377A54BE2F9E6985A8149F19234FD7DDFE22 
{
	bool ___m_value;
};
struct Color_tD001788D726C3A7F1379BEED0260B9591F440C1F 
{
	float ___r;
	float ___g;
	float ___b;
	float ___a;
};
struct Double_tE150EF3D1D43DEE85D533810AB4C742307EEDE5F 
{
	double ___m_value;
};
struct DrivenRectTransformTracker_tFB0706C933E3C68E4F377C204FCEEF091F1EE0B1 
{
	union
	{
		struct
		{
		};
		uint8_t DrivenRectTransformTracker_tFB0706C933E3C68E4F377C204FCEEF091F1EE0B1__padding[1];
	};
};
struct EntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8 
{
	union
	{
		struct
		{
			int32_t ___m_Data;
		};
		uint8_t EntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8__padding[4];
	};
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
struct FourCC_tA6CAA4015BC25A7F1053B6C512202D57A9C994ED 
{
	int32_t ___m_Code;
};
struct Int32_t680FF22E76F6EFAD4375103CBBFFA0421349384C 
{
	int32_t ___m_value;
};
struct IntPtr_t 
{
	void* ___m_value;
};
struct InternedString_t8D62A48CB7D85AAE9CFCCCFB0A77AC2844905735 
{
	String_t* ___m_StringOriginalCase;
	String_t* ___m_StringLowerCase;
};
struct InternedString_t8D62A48CB7D85AAE9CFCCCFB0A77AC2844905735_marshaled_pinvoke
{
	char* ___m_StringOriginalCase;
	char* ___m_StringLowerCase;
};
struct InternedString_t8D62A48CB7D85AAE9CFCCCFB0A77AC2844905735_marshaled_com
{
	Il2CppChar* ___m_StringOriginalCase;
	Il2CppChar* ___m_StringLowerCase;
};
struct LayerMask_t97CB6BDADEDC3D6423C7BCFEA7F86DA2EC6241DB 
{
	int32_t ___m_Mask;
};
struct Mathf_tE284D016E3B297B72311AAD9EB8F0E643F6A4682 
{
	union
	{
		struct
		{
		};
		uint8_t Mathf_tE284D016E3B297B72311AAD9EB8F0E643F6A4682__padding[1];
	};
};
struct Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 
{
	float ___m00;
	float ___m10;
	float ___m20;
	float ___m30;
	float ___m01;
	float ___m11;
	float ___m21;
	float ___m31;
	float ___m02;
	float ___m12;
	float ___m22;
	float ___m32;
	float ___m03;
	float ___m13;
	float ___m23;
	float ___m33;
};
struct PropertyAttribute_t5E0CB5A6CDA6E24CBD4FF26DE3B0C29D8BB54BF0  : public Attribute_tFDA8EFEFB0711976D22474794576DAF28F7440AA
{
	bool ___U3CapplyToCollectionU3Ek__BackingField;
};
struct Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974 
{
	float ___x;
	float ___y;
	float ___z;
	float ___w;
};
struct Single_t4530F2FF86FCB0DC29F35385CA1BD21BE294761C 
{
	float ___m_value;
};
struct SpriteState_tC8199570BE6337FB5C49347C97892B4222E5AACD 
{
	Sprite_tAFF74BC83CD68037494CB0B4F28CBDF8971CAB99* ___m_HighlightedSprite;
	Sprite_tAFF74BC83CD68037494CB0B4F28CBDF8971CAB99* ___m_PressedSprite;
	Sprite_tAFF74BC83CD68037494CB0B4F28CBDF8971CAB99* ___m_SelectedSprite;
	Sprite_tAFF74BC83CD68037494CB0B4F28CBDF8971CAB99* ___m_DisabledSprite;
};
struct SpriteState_tC8199570BE6337FB5C49347C97892B4222E5AACD_marshaled_pinvoke
{
	Sprite_tAFF74BC83CD68037494CB0B4F28CBDF8971CAB99* ___m_HighlightedSprite;
	Sprite_tAFF74BC83CD68037494CB0B4F28CBDF8971CAB99* ___m_PressedSprite;
	Sprite_tAFF74BC83CD68037494CB0B4F28CBDF8971CAB99* ___m_SelectedSprite;
	Sprite_tAFF74BC83CD68037494CB0B4F28CBDF8971CAB99* ___m_DisabledSprite;
};
struct SpriteState_tC8199570BE6337FB5C49347C97892B4222E5AACD_marshaled_com
{
	Sprite_tAFF74BC83CD68037494CB0B4F28CBDF8971CAB99* ___m_HighlightedSprite;
	Sprite_tAFF74BC83CD68037494CB0B4F28CBDF8971CAB99* ___m_PressedSprite;
	Sprite_tAFF74BC83CD68037494CB0B4F28CBDF8971CAB99* ___m_SelectedSprite;
	Sprite_tAFF74BC83CD68037494CB0B4F28CBDF8971CAB99* ___m_DisabledSprite;
};
struct Vector2_t1FD6F485C871E832B347AB2DC8CBA08B739D8DF7 
{
	float ___x;
	float ___y;
};
struct Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 
{
	float ___x;
	float ___y;
	float ___z;
};
struct Vector4_t58B63D32F48C0DBF50DE2C60794C4676C80EDBE3 
{
	float ___x;
	float ___y;
	float ___z;
	float ___w;
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
#pragma pack(push, tp, 1)
struct __StaticArrayInitTypeSizeU3D3566_tEE9DE3B7B7B4A7AC7F1351E4DDF3294D5BE44C82 
{
	union
	{
		struct
		{
			union
			{
			};
		};
		uint8_t __StaticArrayInitTypeSizeU3D3566_tEE9DE3B7B7B4A7AC7F1351E4DDF3294D5BE44C82__padding[3566];
	};
};
#pragma pack(pop, tp)
#pragma pack(push, tp, 1)
struct __StaticArrayInitTypeSizeU3D6293_t0A9B8C4117538EE15A35A0ED91895D0FF5C83D0F 
{
	union
	{
		struct
		{
			union
			{
			};
		};
		uint8_t __StaticArrayInitTypeSizeU3D6293_t0A9B8C4117538EE15A35A0ED91895D0FF5C83D0F__padding[6293];
	};
};
#pragma pack(pop, tp)
struct Enumerator_t609488A9291E792D6A59707FF2BF03FBB727E047 
{
	List_1_t77B94703E05C519A9010DD0614F757F974E1CD8B* ____list;
	int32_t ____index;
	int32_t ____version;
	Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 ____current;
};
struct Bounds_t367E830C64BBF235ED8C3B2F8CF6254FDCAD39C3 
{
	Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 ___m_Center;
	Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 ___m_Extents;
};
struct ColorBlock_tDD7C62E7AFE442652FC98F8D058CE8AE6BFD7C11 
{
	Color_tD001788D726C3A7F1379BEED0260B9591F440C1F ___m_NormalColor;
	Color_tD001788D726C3A7F1379BEED0260B9591F440C1F ___m_HighlightedColor;
	Color_tD001788D726C3A7F1379BEED0260B9591F440C1F ___m_PressedColor;
	Color_tD001788D726C3A7F1379BEED0260B9591F440C1F ___m_SelectedColor;
	Color_tD001788D726C3A7F1379BEED0260B9591F440C1F ___m_DisabledColor;
	float ___m_ColorMultiplier;
	float ___m_FadeDuration;
};
struct FilterMode_t4AD57F1A3FE272D650E0E688BA044AE872BD2A34 
{
	int32_t ___value__;
};
struct HideFlags_tC514182ACEFD3B847988C45D5DB812FF6DB1BF4A 
{
	int32_t ___value__;
};
struct InputStateBlock_t0E05211ACF29A99C0FE7FC9EA7042196BFF1F3B5 
{
	FourCC_tA6CAA4015BC25A7F1053B6C512202D57A9C994ED ___U3CformatU3Ek__BackingField;
	uint32_t ___m_ByteOffset;
	uint32_t ___U3CbitOffsetU3Ek__BackingField;
	uint32_t ___U3CsizeInBitsU3Ek__BackingField;
};
struct LightShape_t538BE3D1AD8C9B537615DAE1C77FA43E26295E91 
{
	int32_t ___value__;
};
struct Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C  : public RuntimeObject
{
	intptr_t ___m_CachedPtr;
};
struct Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C_marshaled_pinvoke
{
	intptr_t ___m_CachedPtr;
};
struct Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C_marshaled_com
{
	intptr_t ___m_CachedPtr;
};
struct Ray_t2B1742D7958DC05BDC3EFC7461D3593E1430DC00 
{
	Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 ___m_Origin;
	Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 ___m_Direction;
};
struct RaycastHit_t6F30BD0B38B56401CA833A1B87BD74F2ACD2F2B5 
{
	Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 ___m_Point;
	Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 ___m_Normal;
	uint32_t ___m_FaceID;
	float ___m_Distance;
	Vector2_t1FD6F485C871E832B347AB2DC8CBA08B739D8DF7 ___m_UV;
	EntityId_t982FBD037EAC5CA077B1602A7EA40E3523AA0FC8 ___m_Collider;
};
struct RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B 
{
	intptr_t ___value;
};
struct TextureFormat_t87A73E4A3850D3410DC211676FC14B94226C1C1D 
{
	int32_t ___value__;
};
struct TextureWrapMode_tF9851343029052ED45668D1C99BAE09B2CCC13AD 
{
	int32_t ___value__;
};
#pragma pack(push, tp, 1)
struct TouchState_tB05024FB4BCE85BAB82D2224AD4BF1E8B4589A97 
{
	union
	{
		struct
		{
			union
			{
				#pragma pack(push, tp, 1)
				struct
				{
					int32_t ___touchId;
				};
				#pragma pack(pop, tp)
				struct
				{
					int32_t ___touchId_forAlignmentOnly;
				};
				#pragma pack(push, tp, 1)
				struct
				{
					char ___position_OffsetPadding[4];
					Vector2_t1FD6F485C871E832B347AB2DC8CBA08B739D8DF7 ___position;
				};
				#pragma pack(pop, tp)
				struct
				{
					char ___position_OffsetPadding_forAlignmentOnly[4];
					Vector2_t1FD6F485C871E832B347AB2DC8CBA08B739D8DF7 ___position_forAlignmentOnly;
				};
				#pragma pack(push, tp, 1)
				struct
				{
					char ___delta_OffsetPadding[12];
					Vector2_t1FD6F485C871E832B347AB2DC8CBA08B739D8DF7 ___delta;
				};
				#pragma pack(pop, tp)
				struct
				{
					char ___delta_OffsetPadding_forAlignmentOnly[12];
					Vector2_t1FD6F485C871E832B347AB2DC8CBA08B739D8DF7 ___delta_forAlignmentOnly;
				};
				#pragma pack(push, tp, 1)
				struct
				{
					char ___pressure_OffsetPadding[20];
					float ___pressure;
				};
				#pragma pack(pop, tp)
				struct
				{
					char ___pressure_OffsetPadding_forAlignmentOnly[20];
					float ___pressure_forAlignmentOnly;
				};
				#pragma pack(push, tp, 1)
				struct
				{
					char ___radius_OffsetPadding[24];
					Vector2_t1FD6F485C871E832B347AB2DC8CBA08B739D8DF7 ___radius;
				};
				#pragma pack(pop, tp)
				struct
				{
					char ___radius_OffsetPadding_forAlignmentOnly[24];
					Vector2_t1FD6F485C871E832B347AB2DC8CBA08B739D8DF7 ___radius_forAlignmentOnly;
				};
				#pragma pack(push, tp, 1)
				struct
				{
					char ___phaseId_OffsetPadding[32];
					uint8_t ___phaseId;
				};
				#pragma pack(pop, tp)
				struct
				{
					char ___phaseId_OffsetPadding_forAlignmentOnly[32];
					uint8_t ___phaseId_forAlignmentOnly;
				};
				#pragma pack(push, tp, 1)
				struct
				{
					char ___tapCount_OffsetPadding[33];
					uint8_t ___tapCount;
				};
				#pragma pack(pop, tp)
				struct
				{
					char ___tapCount_OffsetPadding_forAlignmentOnly[33];
					uint8_t ___tapCount_forAlignmentOnly;
				};
				#pragma pack(push, tp, 1)
				struct
				{
					char ___displayIndex_OffsetPadding[34];
					uint8_t ___displayIndex;
				};
				#pragma pack(pop, tp)
				struct
				{
					char ___displayIndex_OffsetPadding_forAlignmentOnly[34];
					uint8_t ___displayIndex_forAlignmentOnly;
				};
				#pragma pack(push, tp, 1)
				struct
				{
					char ___flags_OffsetPadding[35];
					uint8_t ___flags;
				};
				#pragma pack(pop, tp)
				struct
				{
					char ___flags_OffsetPadding_forAlignmentOnly[35];
					uint8_t ___flags_forAlignmentOnly;
				};
				#pragma pack(push, tp, 1)
				struct
				{
					char ___updateStepCount_OffsetPadding[36];
					uint32_t ___updateStepCount;
				};
				#pragma pack(pop, tp)
				struct
				{
					char ___updateStepCount_OffsetPadding_forAlignmentOnly[36];
					uint32_t ___updateStepCount_forAlignmentOnly;
				};
				#pragma pack(push, tp, 1)
				struct
				{
					char ___startTime_OffsetPadding[40];
					double ___startTime;
				};
				#pragma pack(pop, tp)
				struct
				{
					char ___startTime_OffsetPadding_forAlignmentOnly[40];
					double ___startTime_forAlignmentOnly;
				};
				#pragma pack(push, tp, 1)
				struct
				{
					char ___startPosition_OffsetPadding[48];
					Vector2_t1FD6F485C871E832B347AB2DC8CBA08B739D8DF7 ___startPosition;
				};
				#pragma pack(pop, tp)
				struct
				{
					char ___startPosition_OffsetPadding_forAlignmentOnly[48];
					Vector2_t1FD6F485C871E832B347AB2DC8CBA08B739D8DF7 ___startPosition_forAlignmentOnly;
				};
			};
		};
		uint8_t TouchState_tB05024FB4BCE85BAB82D2224AD4BF1E8B4589A97__padding[56];
	};
};
#pragma pack(pop, tp)
struct TypeCode_tBEF9BE86C8BCF5A6B82F3381219738D27804EF79 
{
	int32_t ___value__;
};
struct VFXBinderAttribute_tB68026C86370BE191A6C1F1DD6002F89B7625E0B  : public PropertyAttribute_t5E0CB5A6CDA6E24CBD4FF26DE3B0C29D8BB54BF0
{
	String_t* ___MenuPath;
};
struct VFXPropertyBindingAttribute_t600577C584AF5B4A36738E462D551C25B663311E  : public PropertyAttribute_t5E0CB5A6CDA6E24CBD4FF26DE3B0C29D8BB54BF0
{
	StringU5BU5D_t7674CD946EC0CE7B3AE0BE70E6EE85F2ECD9F248* ___EditorTypes;
};
struct VFXSpace_t283A1FACD7F083837A82A788944D3E8FADFEAFDC 
{
	int32_t ___value__;
};
struct ControlFlags_t9C297F208DE19CEB00A0560F7FDE59F6A2004132 
{
	int32_t ___value__;
};
struct Mode_t2D49D0E10E2FDA0026278C2400C16033888D0542 
{
	int32_t ___value__;
};
struct Transition_tF856A77C9FAC6D26EA3CA158CF68B739D35397B3 
{
	int32_t ___value__;
};
struct Direction_t4C81D17BB6C089A0EC1C4934525B86E75E693EFA 
{
	int32_t ___value__;
};
struct ToggleTransition_tCE3F3757423A29F22112989C6B4D087850A13ED7 
{
	int32_t ___value__;
};
struct Space_tC1A1B173D794CCED4F69F7B78C6C9D50C69C7FBE 
{
	int32_t ___value__;
};
struct BinderSpace_tA84E57A183C2DAFF23DF3ECA10736A19F5AF5C73 
{
	int32_t ___value__;
};
struct Component_t39FBE53E5EFCF4409111FB22C15FF73717632EC3  : public Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C
{
};
struct GameObject_t76FEDD663AB33C991A9C9A23129337651094216F  : public Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C
{
};
struct Navigation_t4D2E201D65749CF4E104E8AC1232CF1D6F14795C 
{
	int32_t ___m_Mode;
	bool ___m_WrapAround;
	Selectable_t3251808068A17B8E92FB33590A4C2FA66D456712* ___m_SelectOnUp;
	Selectable_t3251808068A17B8E92FB33590A4C2FA66D456712* ___m_SelectOnDown;
	Selectable_t3251808068A17B8E92FB33590A4C2FA66D456712* ___m_SelectOnLeft;
	Selectable_t3251808068A17B8E92FB33590A4C2FA66D456712* ___m_SelectOnRight;
};
struct Navigation_t4D2E201D65749CF4E104E8AC1232CF1D6F14795C_marshaled_pinvoke
{
	int32_t ___m_Mode;
	int32_t ___m_WrapAround;
	Selectable_t3251808068A17B8E92FB33590A4C2FA66D456712* ___m_SelectOnUp;
	Selectable_t3251808068A17B8E92FB33590A4C2FA66D456712* ___m_SelectOnDown;
	Selectable_t3251808068A17B8E92FB33590A4C2FA66D456712* ___m_SelectOnLeft;
	Selectable_t3251808068A17B8E92FB33590A4C2FA66D456712* ___m_SelectOnRight;
};
struct Navigation_t4D2E201D65749CF4E104E8AC1232CF1D6F14795C_marshaled_com
{
	int32_t ___m_Mode;
	int32_t ___m_WrapAround;
	Selectable_t3251808068A17B8E92FB33590A4C2FA66D456712* ___m_SelectOnUp;
	Selectable_t3251808068A17B8E92FB33590A4C2FA66D456712* ___m_SelectOnDown;
	Selectable_t3251808068A17B8E92FB33590A4C2FA66D456712* ___m_SelectOnLeft;
	Selectable_t3251808068A17B8E92FB33590A4C2FA66D456712* ___m_SelectOnRight;
};
struct PrimitiveValue_t1CC37566F40746757D5E3F87474A05909D85C2D4 
{
	union
	{
		#pragma pack(push, tp, 1)
		struct
		{
			int32_t ___m_Type;
		};
		#pragma pack(pop, tp)
		struct
		{
			int32_t ___m_Type_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			char ___m_BoolValue_OffsetPadding[4];
			bool ___m_BoolValue;
		};
		#pragma pack(pop, tp)
		struct
		{
			char ___m_BoolValue_OffsetPadding_forAlignmentOnly[4];
			bool ___m_BoolValue_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			char ___m_CharValue_OffsetPadding[4];
			Il2CppChar ___m_CharValue;
		};
		#pragma pack(pop, tp)
		struct
		{
			char ___m_CharValue_OffsetPadding_forAlignmentOnly[4];
			Il2CppChar ___m_CharValue_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			char ___m_ByteValue_OffsetPadding[4];
			uint8_t ___m_ByteValue;
		};
		#pragma pack(pop, tp)
		struct
		{
			char ___m_ByteValue_OffsetPadding_forAlignmentOnly[4];
			uint8_t ___m_ByteValue_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			char ___m_SByteValue_OffsetPadding[4];
			int8_t ___m_SByteValue;
		};
		#pragma pack(pop, tp)
		struct
		{
			char ___m_SByteValue_OffsetPadding_forAlignmentOnly[4];
			int8_t ___m_SByteValue_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			char ___m_ShortValue_OffsetPadding[4];
			int16_t ___m_ShortValue;
		};
		#pragma pack(pop, tp)
		struct
		{
			char ___m_ShortValue_OffsetPadding_forAlignmentOnly[4];
			int16_t ___m_ShortValue_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			char ___m_UShortValue_OffsetPadding[4];
			uint16_t ___m_UShortValue;
		};
		#pragma pack(pop, tp)
		struct
		{
			char ___m_UShortValue_OffsetPadding_forAlignmentOnly[4];
			uint16_t ___m_UShortValue_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			char ___m_IntValue_OffsetPadding[4];
			int32_t ___m_IntValue;
		};
		#pragma pack(pop, tp)
		struct
		{
			char ___m_IntValue_OffsetPadding_forAlignmentOnly[4];
			int32_t ___m_IntValue_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			char ___m_UIntValue_OffsetPadding[4];
			uint32_t ___m_UIntValue;
		};
		#pragma pack(pop, tp)
		struct
		{
			char ___m_UIntValue_OffsetPadding_forAlignmentOnly[4];
			uint32_t ___m_UIntValue_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			char ___m_LongValue_OffsetPadding[4];
			int64_t ___m_LongValue;
		};
		#pragma pack(pop, tp)
		struct
		{
			char ___m_LongValue_OffsetPadding_forAlignmentOnly[4];
			int64_t ___m_LongValue_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			char ___m_ULongValue_OffsetPadding[4];
			uint64_t ___m_ULongValue;
		};
		#pragma pack(pop, tp)
		struct
		{
			char ___m_ULongValue_OffsetPadding_forAlignmentOnly[4];
			uint64_t ___m_ULongValue_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			char ___m_FloatValue_OffsetPadding[4];
			float ___m_FloatValue;
		};
		#pragma pack(pop, tp)
		struct
		{
			char ___m_FloatValue_OffsetPadding_forAlignmentOnly[4];
			float ___m_FloatValue_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			char ___m_DoubleValue_OffsetPadding[4];
			double ___m_DoubleValue;
		};
		#pragma pack(pop, tp)
		struct
		{
			char ___m_DoubleValue_OffsetPadding_forAlignmentOnly[4];
			double ___m_DoubleValue_forAlignmentOnly;
		};
	};
};
struct PrimitiveValue_t1CC37566F40746757D5E3F87474A05909D85C2D4_marshaled_pinvoke
{
	union
	{
		#pragma pack(push, tp, 1)
		struct
		{
			int32_t ___m_Type;
		};
		#pragma pack(pop, tp)
		struct
		{
			int32_t ___m_Type_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			char ___m_BoolValue_OffsetPadding[4];
			int32_t ___m_BoolValue;
		};
		#pragma pack(pop, tp)
		struct
		{
			char ___m_BoolValue_OffsetPadding_forAlignmentOnly[4];
			int32_t ___m_BoolValue_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			char ___m_CharValue_OffsetPadding[4];
			uint8_t ___m_CharValue;
		};
		#pragma pack(pop, tp)
		struct
		{
			char ___m_CharValue_OffsetPadding_forAlignmentOnly[4];
			uint8_t ___m_CharValue_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			char ___m_ByteValue_OffsetPadding[4];
			uint8_t ___m_ByteValue;
		};
		#pragma pack(pop, tp)
		struct
		{
			char ___m_ByteValue_OffsetPadding_forAlignmentOnly[4];
			uint8_t ___m_ByteValue_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			char ___m_SByteValue_OffsetPadding[4];
			int8_t ___m_SByteValue;
		};
		#pragma pack(pop, tp)
		struct
		{
			char ___m_SByteValue_OffsetPadding_forAlignmentOnly[4];
			int8_t ___m_SByteValue_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			char ___m_ShortValue_OffsetPadding[4];
			int16_t ___m_ShortValue;
		};
		#pragma pack(pop, tp)
		struct
		{
			char ___m_ShortValue_OffsetPadding_forAlignmentOnly[4];
			int16_t ___m_ShortValue_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			char ___m_UShortValue_OffsetPadding[4];
			uint16_t ___m_UShortValue;
		};
		#pragma pack(pop, tp)
		struct
		{
			char ___m_UShortValue_OffsetPadding_forAlignmentOnly[4];
			uint16_t ___m_UShortValue_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			char ___m_IntValue_OffsetPadding[4];
			int32_t ___m_IntValue;
		};
		#pragma pack(pop, tp)
		struct
		{
			char ___m_IntValue_OffsetPadding_forAlignmentOnly[4];
			int32_t ___m_IntValue_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			char ___m_UIntValue_OffsetPadding[4];
			uint32_t ___m_UIntValue;
		};
		#pragma pack(pop, tp)
		struct
		{
			char ___m_UIntValue_OffsetPadding_forAlignmentOnly[4];
			uint32_t ___m_UIntValue_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			char ___m_LongValue_OffsetPadding[4];
			int64_t ___m_LongValue;
		};
		#pragma pack(pop, tp)
		struct
		{
			char ___m_LongValue_OffsetPadding_forAlignmentOnly[4];
			int64_t ___m_LongValue_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			char ___m_ULongValue_OffsetPadding[4];
			uint64_t ___m_ULongValue;
		};
		#pragma pack(pop, tp)
		struct
		{
			char ___m_ULongValue_OffsetPadding_forAlignmentOnly[4];
			uint64_t ___m_ULongValue_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			char ___m_FloatValue_OffsetPadding[4];
			float ___m_FloatValue;
		};
		#pragma pack(pop, tp)
		struct
		{
			char ___m_FloatValue_OffsetPadding_forAlignmentOnly[4];
			float ___m_FloatValue_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			char ___m_DoubleValue_OffsetPadding[4];
			double ___m_DoubleValue;
		};
		#pragma pack(pop, tp)
		struct
		{
			char ___m_DoubleValue_OffsetPadding_forAlignmentOnly[4];
			double ___m_DoubleValue_forAlignmentOnly;
		};
	};
};
struct PrimitiveValue_t1CC37566F40746757D5E3F87474A05909D85C2D4_marshaled_com
{
	union
	{
		#pragma pack(push, tp, 1)
		struct
		{
			int32_t ___m_Type;
		};
		#pragma pack(pop, tp)
		struct
		{
			int32_t ___m_Type_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			char ___m_BoolValue_OffsetPadding[4];
			int32_t ___m_BoolValue;
		};
		#pragma pack(pop, tp)
		struct
		{
			char ___m_BoolValue_OffsetPadding_forAlignmentOnly[4];
			int32_t ___m_BoolValue_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			char ___m_CharValue_OffsetPadding[4];
			uint8_t ___m_CharValue;
		};
		#pragma pack(pop, tp)
		struct
		{
			char ___m_CharValue_OffsetPadding_forAlignmentOnly[4];
			uint8_t ___m_CharValue_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			char ___m_ByteValue_OffsetPadding[4];
			uint8_t ___m_ByteValue;
		};
		#pragma pack(pop, tp)
		struct
		{
			char ___m_ByteValue_OffsetPadding_forAlignmentOnly[4];
			uint8_t ___m_ByteValue_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			char ___m_SByteValue_OffsetPadding[4];
			int8_t ___m_SByteValue;
		};
		#pragma pack(pop, tp)
		struct
		{
			char ___m_SByteValue_OffsetPadding_forAlignmentOnly[4];
			int8_t ___m_SByteValue_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			char ___m_ShortValue_OffsetPadding[4];
			int16_t ___m_ShortValue;
		};
		#pragma pack(pop, tp)
		struct
		{
			char ___m_ShortValue_OffsetPadding_forAlignmentOnly[4];
			int16_t ___m_ShortValue_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			char ___m_UShortValue_OffsetPadding[4];
			uint16_t ___m_UShortValue;
		};
		#pragma pack(pop, tp)
		struct
		{
			char ___m_UShortValue_OffsetPadding_forAlignmentOnly[4];
			uint16_t ___m_UShortValue_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			char ___m_IntValue_OffsetPadding[4];
			int32_t ___m_IntValue;
		};
		#pragma pack(pop, tp)
		struct
		{
			char ___m_IntValue_OffsetPadding_forAlignmentOnly[4];
			int32_t ___m_IntValue_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			char ___m_UIntValue_OffsetPadding[4];
			uint32_t ___m_UIntValue;
		};
		#pragma pack(pop, tp)
		struct
		{
			char ___m_UIntValue_OffsetPadding_forAlignmentOnly[4];
			uint32_t ___m_UIntValue_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			char ___m_LongValue_OffsetPadding[4];
			int64_t ___m_LongValue;
		};
		#pragma pack(pop, tp)
		struct
		{
			char ___m_LongValue_OffsetPadding_forAlignmentOnly[4];
			int64_t ___m_LongValue_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			char ___m_ULongValue_OffsetPadding[4];
			uint64_t ___m_ULongValue;
		};
		#pragma pack(pop, tp)
		struct
		{
			char ___m_ULongValue_OffsetPadding_forAlignmentOnly[4];
			uint64_t ___m_ULongValue_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			char ___m_FloatValue_OffsetPadding[4];
			float ___m_FloatValue;
		};
		#pragma pack(pop, tp)
		struct
		{
			char ___m_FloatValue_OffsetPadding_forAlignmentOnly[4];
			float ___m_FloatValue_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			char ___m_DoubleValue_OffsetPadding[4];
			double ___m_DoubleValue;
		};
		#pragma pack(pop, tp)
		struct
		{
			char ___m_DoubleValue_OffsetPadding_forAlignmentOnly[4];
			double ___m_DoubleValue_forAlignmentOnly;
		};
	};
};
struct TerrainData_t615A68EAC648066681875D47FC641496D12F2E24  : public Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C
{
};
struct Texture_t791CBB51219779964E0E8A2ED7C1AA5F92A4A700  : public Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C
{
};
struct Type_t  : public MemberInfo_t
{
	RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B ____impl;
};
struct VisualEffectObject_t05DA11D8B79185E02344C115C8075FC95E7623BE  : public Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C
{
};
struct Behaviour_t01970CFBBA658497AE30F311C447DB0440BAB7FA  : public Component_t39FBE53E5EFCF4409111FB22C15FF73717632EC3
{
};
struct Collider_t1CC3163924FCD6C4CC2E816373A929C1E3D55E76  : public Component_t39FBE53E5EFCF4409111FB22C15FF73717632EC3
{
};
struct InputControl_t74F06B623518F992BF8E38656A5E0857169E3E2E  : public RuntimeObject
{
	InputStateBlock_t0E05211ACF29A99C0FE7FC9EA7042196BFF1F3B5 ___m_StateBlock;
	InternedString_t8D62A48CB7D85AAE9CFCCCFB0A77AC2844905735 ___m_Name;
	String_t* ___m_Path;
	String_t* ___m_DisplayName;
	String_t* ___m_DisplayNameFromLayout;
	String_t* ___m_ShortDisplayName;
	String_t* ___m_ShortDisplayNameFromLayout;
	InternedString_t8D62A48CB7D85AAE9CFCCCFB0A77AC2844905735 ___m_Layout;
	InternedString_t8D62A48CB7D85AAE9CFCCCFB0A77AC2844905735 ___m_Variants;
	InputDevice_t8BCF67533E872A75779C24C93D1D7085B72D364B* ___m_Device;
	InputControl_t74F06B623518F992BF8E38656A5E0857169E3E2E* ___m_Parent;
	int32_t ___m_UsageCount;
	int32_t ___m_UsageStartIndex;
	int32_t ___m_AliasCount;
	int32_t ___m_AliasStartIndex;
	int32_t ___m_ChildCount;
	int32_t ___m_ChildStartIndex;
	int32_t ___m_ControlFlags;
	bool ___m_CachedValueIsStale;
	bool ___m_UnprocessedCachedValueIsStale;
	PrimitiveValue_t1CC37566F40746757D5E3F87474A05909D85C2D4 ___m_DefaultState;
	PrimitiveValue_t1CC37566F40746757D5E3F87474A05909D85C2D4 ___m_MinValue;
	PrimitiveValue_t1CC37566F40746757D5E3F87474A05909D85C2D4 ___m_MaxValue;
	FourCC_tA6CAA4015BC25A7F1053B6C512202D57A9C994ED ___m_OptimizedControlDataType;
};
struct RenderTexture_tBA90C4C3AD9EECCFDDCC632D97C29FAB80D60D27  : public Texture_t791CBB51219779964E0E8A2ED7C1AA5F92A4A700
{
};
struct Texture2D_tE6505BC111DD8A424A9DBE8E05D7D09E11FFFCF4  : public Texture_t791CBB51219779964E0E8A2ED7C1AA5F92A4A700
{
};
struct Transform_tB27202C6F4E36D225EE28A13E4D662BF99785DB1  : public Component_t39FBE53E5EFCF4409111FB22C15FF73717632EC3
{
};
struct VisualEffectAsset_t0CD57FD9980CD68033BD96DA356697F92A69E688  : public VisualEffectObject_t05DA11D8B79185E02344C115C8075FC95E7623BE
{
};
struct InputControl_1_t8328183BE585255AE6A8A1F8D5C9DEEDE891A3B3  : public InputControl_t74F06B623518F992BF8E38656A5E0857169E3E2E
{
	InlinedArray_1_tD4AEEFFEC2CBF49D196A92D16CE8DA15F1532FCD ___m_ProcessorStack;
	TouchState_tB05024FB4BCE85BAB82D2224AD4BF1E8B4589A97 ___m_CachedValue;
	TouchState_tB05024FB4BCE85BAB82D2224AD4BF1E8B4589A97 ___m_UnprocessedCachedValue;
	bool ___evaluateProcessorsEveryRead;
};
struct Light_t1E68479B7782AF2050FAA02A5DC612FD034F18F3  : public Behaviour_t01970CFBBA658497AE30F311C447DB0440BAB7FA
{
	int32_t ___U3CshapeU3Ek__BackingField;
	int32_t ___m_BakedIndex;
};
struct MonoBehaviour_t532A11E69716D348D8AA7F854AFCBFCB8AD17F71  : public Behaviour_t01970CFBBA658497AE30F311C447DB0440BAB7FA
{
	CancellationTokenSource_tAAE1E0033BCFC233801F8CB4CED5C852B350CB7B* ___m_CancellationTokenSource;
};
struct SphereCollider_tBA111C542CE97F6873DE742757213D6265C7D275  : public Collider_t1CC3163924FCD6C4CC2E816373A929C1E3D55E76
{
};
struct Terrain_t7F309492F67238DBFBC4566F47385B2A665CF667  : public Behaviour_t01970CFBBA658497AE30F311C447DB0440BAB7FA
{
};
struct VisualEffect_t8CF4F0E13AEBF27224BB1323CC103666EDBBCFB0  : public Behaviour_t01970CFBBA658497AE30F311C447DB0440BAB7FA
{
	VFXEventAttribute_tA764A595356174BA39C782A8A64B47B39FB3F518* ___m_cachedEventAttribute;
	Action_1_tBAC06A706FB1F9A81AEF7EDF6202FC51D1E52C3A* ___outputEventReceived;
};
struct TouchControl_t3A64A795FF61A8B27E5EBD61632F23AFCC725079  : public InputControl_1_t8328183BE585255AE6A8A1F8D5C9DEEDE891A3B3
{
	TouchPressControl_tD5BD68D9FDEAC889591DD7A499864893A43C7CEB* ___U3CpressU3Ek__BackingField;
	IntegerControl_tA24544EFF42204852F638FF5147F754962C997AB* ___U3CdisplayIndexU3Ek__BackingField;
	IntegerControl_tA24544EFF42204852F638FF5147F754962C997AB* ___U3CtouchIdU3Ek__BackingField;
	Vector2Control_t8D1B4021A1D82671AF916D3C0A476AA94E46A432* ___U3CpositionU3Ek__BackingField;
	DeltaControl_t63053AF5E0CD02B62F3CDE79821E4A12F72D573B* ___U3CdeltaU3Ek__BackingField;
	AxisControl_tD6613A2445A3C2BFA22C77E16CA3201AF72354A7* ___U3CpressureU3Ek__BackingField;
	Vector2Control_t8D1B4021A1D82671AF916D3C0A476AA94E46A432* ___U3CradiusU3Ek__BackingField;
	TouchPhaseControl_t4652C6E0EBFAFA85A31607769876B02C1C9FCAEC* ___U3CphaseU3Ek__BackingField;
	ButtonControl_t85949109B98AAF5B7ADC0285F0EC98A61EC88ECF* ___U3CindirectTouchU3Ek__BackingField;
	ButtonControl_t85949109B98AAF5B7ADC0285F0EC98A61EC88ECF* ___U3CtapU3Ek__BackingField;
	IntegerControl_tA24544EFF42204852F638FF5147F754962C997AB* ___U3CtapCountU3Ek__BackingField;
	DoubleControl_t3707A6260424A2F3636669A2BAFF5CB5E91D2486* ___U3CstartTimeU3Ek__BackingField;
	Vector2Control_t8D1B4021A1D82671AF916D3C0A476AA94E46A432* ___U3CstartPositionU3Ek__BackingField;
};
struct UIBehaviour_tB9D4295827BD2EEDEF0749200C6CA7090C742A9D  : public MonoBehaviour_t532A11E69716D348D8AA7F854AFCBFCB8AD17F71
{
};
struct VFXBinderBase_t8BC197F500B3A144DF76E77C48FA239C04CDA19C  : public MonoBehaviour_t532A11E69716D348D8AA7F854AFCBFCB8AD17F71
{
	VFXPropertyBinder_t761F7FEC5308F4D9ABF7F0A5A3FAAAA6FE043C0F* ___binder;
};
struct VFXPropertyBinder_t761F7FEC5308F4D9ABF7F0A5A3FAAAA6FE043C0F  : public MonoBehaviour_t532A11E69716D348D8AA7F854AFCBFCB8AD17F71
{
	bool ___m_ExecuteInEditor;
	List_1_t5A6730315AA46B39985DF97B343C4E4ACEF77B42* ___m_Bindings;
	VisualEffect_t8CF4F0E13AEBF27224BB1323CC103666EDBBCFB0* ___m_VisualEffect;
};
struct Selectable_t3251808068A17B8E92FB33590A4C2FA66D456712  : public UIBehaviour_tB9D4295827BD2EEDEF0749200C6CA7090C742A9D
{
	bool ___m_EnableCalled;
	Navigation_t4D2E201D65749CF4E104E8AC1232CF1D6F14795C ___m_Navigation;
	int32_t ___m_Transition;
	ColorBlock_tDD7C62E7AFE442652FC98F8D058CE8AE6BFD7C11 ___m_Colors;
	SpriteState_tC8199570BE6337FB5C49347C97892B4222E5AACD ___m_SpriteState;
	AnimationTriggers_tA0DC06F89C5280C6DD972F6F4C8A56D7F4F79074* ___m_AnimationTriggers;
	bool ___m_Interactable;
	Graphic_tCBFCA4585A19E2B75465AECFEAC43F4016BF7931* ___m_TargetGraphic;
	bool ___m_GroupsAllowInteraction;
	int32_t ___m_CurrentIndex;
	bool ___U3CisPointerInsideU3Ek__BackingField;
	bool ___U3CisPointerDownU3Ek__BackingField;
	bool ___U3ChasSelectionU3Ek__BackingField;
	List_1_t2CDCA768E7F493F5EDEBC75AEB200FD621354E35* ___m_CanvasGroupCache;
};
struct VFXLightBinder_t5FDD7FBBDBFDB8A2AFCAEC4B46460F37EE4A672A  : public VFXBinderBase_t8BC197F500B3A144DF76E77C48FA239C04CDA19C
{
	ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* ___m_ColorProperty;
	ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* ___m_BrightnessProperty;
	ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* ___m_RadiusProperty;
	Light_t1E68479B7782AF2050FAA02A5DC612FD034F18F3* ___Target;
	bool ___BindColor;
	bool ___BindBrightness;
	bool ___BindRadius;
};
struct VFXMultiplePositionBinder_t1891AAD3420705E4BA559CFF54038E8F6B27947F  : public VFXBinderBase_t8BC197F500B3A144DF76E77C48FA239C04CDA19C
{
	ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* ___PositionMapProperty;
	ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* ___PositionCountProperty;
	GameObjectU5BU5D_tFF67550DFCE87096D7A3734EA15B75896B2722CF* ___Targets;
	bool ___EveryFrame;
	Texture2D_tE6505BC111DD8A424A9DBE8E05D7D09E11FFFCF4* ___positionMap;
	int32_t ___count;
};
struct VFXRaycastBinder_tD681421294476C02004DF02A34F958796887ED17  : public VFXBinderBase_t8BC197F500B3A144DF76E77C48FA239C04CDA19C
{
	ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* ___m_TargetPosition;
	ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* ___m_TargetNormal;
	ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* ___m_TargetHit;
	ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* ___m_TargetPosition_position;
	ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* ___m_TargetNormal_direction;
	GameObject_t76FEDD663AB33C991A9C9A23129337651094216F* ___RaycastSource;
	Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 ___RaycastDirection;
	int32_t ___RaycastDirectionSpace;
	LayerMask_t97CB6BDADEDC3D6423C7BCFEA7F86DA2EC6241DB ___Layers;
	float ___MaxDistance;
	RaycastHit_t6F30BD0B38B56401CA833A1B87BD74F2ACD2F2B5 ___m_HitInfo;
};
struct VFXSpaceableBinder_t0C24970B59F1F8E2DA67992F0E90DF951BF91706  : public VFXBinderBase_t8BC197F500B3A144DF76E77C48FA239C04CDA19C
{
	int32_t ___Space;
};
struct VFXTerrainBinder_tBB926D3EE00EA5123B213E37F559F7DDDBFC173F  : public VFXBinderBase_t8BC197F500B3A144DF76E77C48FA239C04CDA19C
{
	ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* ___m_Property;
	Terrain_t7F309492F67238DBFBC4566F47385B2A665CF667* ___Terrain;
	ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* ___Terrain_Bounds_center;
	ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* ___Terrain_Bounds_size;
	ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* ___Terrain_HeightMap;
	ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* ___Terrain_Height;
};
struct VFXUIDropdownBinder_tE9AA1FDC5FA865CD3412BE969A5A73A8ADA598EE  : public VFXBinderBase_t8BC197F500B3A144DF76E77C48FA239C04CDA19C
{
	ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* ___m_Property;
	Dropdown_t54C0BDC1441E058BE37E796F68886671C270EF89* ___Target;
};
struct VFXUISliderBinder_t1A3AF1ABC9C5F1E0AE11E306E67AA2711FFF9516  : public VFXBinderBase_t8BC197F500B3A144DF76E77C48FA239C04CDA19C
{
	ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* ___m_Property;
	Slider_t87EA570E3D6556CABF57456C2F3873FFD86E652F* ___Target;
};
struct VFXUIToggleBinder_t16637477616EDAD883FC73354821CFF850463E1E  : public VFXBinderBase_t8BC197F500B3A144DF76E77C48FA239C04CDA19C
{
	ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* ___m_Property;
	Toggle_tBF13F3EBA485E06826FD8A38F4B4C1380DF21A1F* ___Target;
};
struct Dropdown_t54C0BDC1441E058BE37E796F68886671C270EF89  : public Selectable_t3251808068A17B8E92FB33590A4C2FA66D456712
{
	RectTransform_t6C5DA5E41A89E0F488B001E45E58963480E543A5* ___m_Template;
	Text_tD60B2346DAA6666BF0D822FF607F0B220C2B9E62* ___m_CaptionText;
	Image_tBC1D03F63BF71132E9A5E472B8742F172A011E7E* ___m_CaptionImage;
	Text_tD60B2346DAA6666BF0D822FF607F0B220C2B9E62* ___m_ItemText;
	Image_tBC1D03F63BF71132E9A5E472B8742F172A011E7E* ___m_ItemImage;
	int32_t ___m_Value;
	OptionDataList_t53255477D0A9C6980AB48693A520EFBC94DFFB96* ___m_Options;
	DropdownEvent_t8A008B010A742724CFC93576D6976E474BB13059* ___m_OnValueChanged;
	float ___m_AlphaFadeSpeed;
	GameObject_t76FEDD663AB33C991A9C9A23129337651094216F* ___m_Dropdown;
	GameObject_t76FEDD663AB33C991A9C9A23129337651094216F* ___m_Blocker;
	List_1_t89B39292AD45371F7FDCB295AAE956D33588BC6E* ___m_Items;
	TweenRunner_1_t830EC096236A3CEC7189DFA6E0B2E74C5C97780B* ___m_AlphaTweenRunner;
	bool ___validTemplate;
};
struct Slider_t87EA570E3D6556CABF57456C2F3873FFD86E652F  : public Selectable_t3251808068A17B8E92FB33590A4C2FA66D456712
{
	RectTransform_t6C5DA5E41A89E0F488B001E45E58963480E543A5* ___m_FillRect;
	RectTransform_t6C5DA5E41A89E0F488B001E45E58963480E543A5* ___m_HandleRect;
	int32_t ___m_Direction;
	float ___m_MinValue;
	float ___m_MaxValue;
	bool ___m_WholeNumbers;
	float ___m_Value;
	SliderEvent_t92A82EF6C62E15AF92B640FE2D960E877E8C6555* ___m_OnValueChanged;
	Image_tBC1D03F63BF71132E9A5E472B8742F172A011E7E* ___m_FillImage;
	Transform_tB27202C6F4E36D225EE28A13E4D662BF99785DB1* ___m_FillTransform;
	RectTransform_t6C5DA5E41A89E0F488B001E45E58963480E543A5* ___m_FillContainerRect;
	Transform_tB27202C6F4E36D225EE28A13E4D662BF99785DB1* ___m_HandleTransform;
	RectTransform_t6C5DA5E41A89E0F488B001E45E58963480E543A5* ___m_HandleContainerRect;
	Vector2_t1FD6F485C871E832B347AB2DC8CBA08B739D8DF7 ___m_Offset;
	DrivenRectTransformTracker_tFB0706C933E3C68E4F377C204FCEEF091F1EE0B1 ___m_Tracker;
	bool ___m_DelayedUpdateVisuals;
};
struct Toggle_tBF13F3EBA485E06826FD8A38F4B4C1380DF21A1F  : public Selectable_t3251808068A17B8E92FB33590A4C2FA66D456712
{
	int32_t ___toggleTransition;
	Graphic_tCBFCA4585A19E2B75465AECFEAC43F4016BF7931* ___graphic;
	ToggleGroup_tF2E6FE7D4B17BDBF82462715CFB57C4FDE0A2A2C* ___m_Group;
	ToggleEvent_t88B31268F9D6D1882E4F921B14704FB9F7047F02* ___onValueChanged;
	bool ___m_IsOn;
};
struct VFXPlaneBinder_t52D64377F31144687E6984CB470F8F20DF50AB58  : public VFXSpaceableBinder_t0C24970B59F1F8E2DA67992F0E90DF951BF91706
{
	ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* ___m_Property;
	Transform_tB27202C6F4E36D225EE28A13E4D662BF99785DB1* ___Target;
	ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* ___Position;
	ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* ___Normal;
};
struct VFXPositionBinder_t4CD7239B0C1EA2DA36502BF5D324FAF39632F8A4  : public VFXSpaceableBinder_t0C24970B59F1F8E2DA67992F0E90DF951BF91706
{
	ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* ___m_Property;
	Transform_tB27202C6F4E36D225EE28A13E4D662BF99785DB1* ___Target;
};
struct VFXPreviousPositionBinder_t287E40B0B297056DD27888F72D017CED6BC1F524  : public VFXSpaceableBinder_t0C24970B59F1F8E2DA67992F0E90DF951BF91706
{
	ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* ___m_Property;
	Transform_tB27202C6F4E36D225EE28A13E4D662BF99785DB1* ___Target;
	Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 ___oldPosition;
};
struct VFXSphereBinder_t1EC83273D815BCA9C53CD4F7D03C8EAE35811624  : public VFXSpaceableBinder_t0C24970B59F1F8E2DA67992F0E90DF951BF91706
{
	ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* ___m_Property;
	SphereCollider_tBA111C542CE97F6873DE742757213D6265C7D275* ___Target;
	ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* ___m_Old_Center;
	ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* ___m_New_Center;
	ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* ___m_Radius;
};
struct VFXTransformBinder_t2658F13D9C6472FD34C1B8276A298E2F71FE042A  : public VFXSpaceableBinder_t0C24970B59F1F8E2DA67992F0E90DF951BF91706
{
	ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* ___m_Property;
	Transform_tB27202C6F4E36D225EE28A13E4D662BF99785DB1* ___Target;
	ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* ___Position;
	ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* ___Angles;
	ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* ___Scale;
};
struct VFXVelocityBinder_t70DB0B43C2865624A6C9127476F12BC5B6EA31FC  : public VFXSpaceableBinder_t0C24970B59F1F8E2DA67992F0E90DF951BF91706
{
	ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* ___m_Property;
	Transform_tB27202C6F4E36D225EE28A13E4D662BF99785DB1* ___Target;
	float ___m_PreviousTime;
	Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 ___m_PreviousPosition;
};
struct EmptyArray_1_tDF0DD7256B115243AA6BD5558417387A734240EE_StaticFields
{
	ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___Value;
};
struct List_1_t242CDEAEC9C92000DA96982CDB9D592DDE2AADAF_StaticFields
{
	ColorU5BU5D_t612261CF293F6FFC3D80AB52259FF0DC2B2CC389* ___s_emptyArray;
};
struct List_1_tA239CB83DE5615F348BB0507E45F490F4F7C9A8D_StaticFields
{
	ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___s_emptyArray;
};
struct List_1_t5A6730315AA46B39985DF97B343C4E4ACEF77B42_StaticFields
{
	VFXBinderBaseU5BU5D_t1ECF22FF6432C40931AF7B0DEC149B606A33E247* ___s_emptyArray;
};
struct List_1_t77B94703E05C519A9010DD0614F757F974E1CD8B_StaticFields
{
	Vector3U5BU5D_tFF1859CCE176131B909E2044F76443064254679C* ___s_emptyArray;
};
struct U3CPrivateImplementationDetailsU3E_tE3CD203A299B6432ED139A111B41EA2D218822C7_StaticFields
{
	__StaticArrayInitTypeSizeU3D6293_t0A9B8C4117538EE15A35A0ED91895D0FF5C83D0F ___E3E55352DF7FEA48D9BD25C04FAC8225F8900BC1C792359C00530F879FDF70DC;
	__StaticArrayInitTypeSizeU3D3566_tEE9DE3B7B7B4A7AC7F1351E4DDF3294D5BE44C82 ___EBB49FC4B9DF9593EFE6354EC463D0B2B218C84ECF16B55C0960D1B15632F5A6;
};
struct String_t_StaticFields
{
	String_t* ___Empty;
};
struct U3CU3Ec_t32602C6EC431C90BF3D26B05836A825F00E5B9DD_StaticFields
{
	U3CU3Ec_t32602C6EC431C90BF3D26B05836A825F00E5B9DD* ___U3CU3E9;
	Func_2_tB530FD2BAF3A03B6BE3CE2AA56B6440DBFD63F0A* ___U3CU3E9__20_0;
};
struct Boolean_t09A6377A54BE2F9E6985A8149F19234FD7DDFE22_StaticFields
{
	String_t* ___TrueString;
	String_t* ___FalseString;
};
struct Mathf_tE284D016E3B297B72311AAD9EB8F0E643F6A4682_StaticFields
{
	float ___Epsilon;
};
struct Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6_StaticFields
{
	Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 ___zeroMatrix;
	Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 ___identityMatrix;
};
struct Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974_StaticFields
{
	Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974 ___identityQuaternion;
};
struct Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2_StaticFields
{
	Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 ___zeroVector;
	Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 ___oneVector;
	Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 ___upVector;
	Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 ___downVector;
	Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 ___leftVector;
	Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 ___rightVector;
	Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 ___forwardVector;
	Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 ___backVector;
	Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 ___positiveInfinityVector;
	Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 ___negativeInfinityVector;
};
struct Vector4_t58B63D32F48C0DBF50DE2C60794C4676C80EDBE3_StaticFields
{
	Vector4_t58B63D32F48C0DBF50DE2C60794C4676C80EDBE3 ___zeroVector;
	Vector4_t58B63D32F48C0DBF50DE2C60794C4676C80EDBE3 ___oneVector;
	Vector4_t58B63D32F48C0DBF50DE2C60794C4676C80EDBE3 ___positiveInfinityVector;
	Vector4_t58B63D32F48C0DBF50DE2C60794C4676C80EDBE3 ___negativeInfinityVector;
};
struct Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C_StaticFields
{
	int32_t ___OffsetOfInstanceIDInCPlusPlusObject;
};
struct TerrainData_t615A68EAC648066681875D47FC641496D12F2E24_StaticFields
{
	int32_t ___k_MaximumResolution;
	int32_t ___k_MinimumDetailResolutionPerPatch;
	int32_t ___k_MaximumDetailResolutionPerPatch;
	int32_t ___k_MaximumDetailPatchCount;
	int32_t ___k_MinimumAlphamapResolution;
	int32_t ___k_MaximumAlphamapResolution;
	int32_t ___k_MinimumBaseMapResolution;
	int32_t ___k_MaximumBaseMapResolution;
};
struct Texture_t791CBB51219779964E0E8A2ED7C1AA5F92A4A700_StaticFields
{
	int32_t ___GenerateAllMips;
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
struct VisualEffectAsset_t0CD57FD9980CD68033BD96DA356697F92A69E688_StaticFields
{
	int32_t ___PlayEventID;
	int32_t ___StopEventID;
};
struct Dropdown_t54C0BDC1441E058BE37E796F68886671C270EF89_StaticFields
{
	OptionData_t68DC820D58A3ABBAE844326B15A7F14D48FAE55F* ___s_NoOptionData;
};
struct VFXVelocityBinder_t70DB0B43C2865624A6C9127476F12BC5B6EA31FC_StaticFields
{
	float ___invalidPreviousTime;
};
#ifdef __clang__
#pragma clang diagnostic pop
#endif
struct GameObjectU5BU5D_tFF67550DFCE87096D7A3734EA15B75896B2722CF  : public RuntimeArray
{
	ALIGN_FIELD (8) GameObject_t76FEDD663AB33C991A9C9A23129337651094216F* m_Items[1];

	inline GameObject_t76FEDD663AB33C991A9C9A23129337651094216F* GetAt(il2cpp_array_size_t index) const
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		return m_Items[index];
	}
	inline GameObject_t76FEDD663AB33C991A9C9A23129337651094216F** GetAddressAt(il2cpp_array_size_t index)
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		return m_Items + index;
	}
	inline void SetAt(il2cpp_array_size_t index, GameObject_t76FEDD663AB33C991A9C9A23129337651094216F* value)
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		m_Items[index] = value;
		Il2CppCodeGenWriteBarrier((void**)m_Items + index, (void*)value);
	}
	inline GameObject_t76FEDD663AB33C991A9C9A23129337651094216F* GetAtUnchecked(il2cpp_array_size_t index) const
	{
		return m_Items[index];
	}
	inline GameObject_t76FEDD663AB33C991A9C9A23129337651094216F** GetAddressAtUnchecked(il2cpp_array_size_t index)
	{
		return m_Items + index;
	}
	inline void SetAtUnchecked(il2cpp_array_size_t index, GameObject_t76FEDD663AB33C991A9C9A23129337651094216F* value)
	{
		m_Items[index] = value;
		Il2CppCodeGenWriteBarrier((void**)m_Items + index, (void*)value);
	}
};
struct ColorU5BU5D_t612261CF293F6FFC3D80AB52259FF0DC2B2CC389  : public RuntimeArray
{
	ALIGN_FIELD (8) Color_tD001788D726C3A7F1379BEED0260B9591F440C1F m_Items[1];

	inline Color_tD001788D726C3A7F1379BEED0260B9591F440C1F GetAt(il2cpp_array_size_t index) const
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		return m_Items[index];
	}
	inline Color_tD001788D726C3A7F1379BEED0260B9591F440C1F* GetAddressAt(il2cpp_array_size_t index)
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		return m_Items + index;
	}
	inline void SetAt(il2cpp_array_size_t index, Color_tD001788D726C3A7F1379BEED0260B9591F440C1F value)
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		m_Items[index] = value;
	}
	inline Color_tD001788D726C3A7F1379BEED0260B9591F440C1F GetAtUnchecked(il2cpp_array_size_t index) const
	{
		return m_Items[index];
	}
	inline Color_tD001788D726C3A7F1379BEED0260B9591F440C1F* GetAddressAtUnchecked(il2cpp_array_size_t index)
	{
		return m_Items + index;
	}
	inline void SetAtUnchecked(il2cpp_array_size_t index, Color_tD001788D726C3A7F1379BEED0260B9591F440C1F value)
	{
		m_Items[index] = value;
	}
};
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
struct VFXBinderBaseU5BU5D_t1ECF22FF6432C40931AF7B0DEC149B606A33E247  : public RuntimeArray
{
	ALIGN_FIELD (8) VFXBinderBase_t8BC197F500B3A144DF76E77C48FA239C04CDA19C* m_Items[1];

	inline VFXBinderBase_t8BC197F500B3A144DF76E77C48FA239C04CDA19C* GetAt(il2cpp_array_size_t index) const
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		return m_Items[index];
	}
	inline VFXBinderBase_t8BC197F500B3A144DF76E77C48FA239C04CDA19C** GetAddressAt(il2cpp_array_size_t index)
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		return m_Items + index;
	}
	inline void SetAt(il2cpp_array_size_t index, VFXBinderBase_t8BC197F500B3A144DF76E77C48FA239C04CDA19C* value)
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		m_Items[index] = value;
		Il2CppCodeGenWriteBarrier((void**)m_Items + index, (void*)value);
	}
	inline VFXBinderBase_t8BC197F500B3A144DF76E77C48FA239C04CDA19C* GetAtUnchecked(il2cpp_array_size_t index) const
	{
		return m_Items[index];
	}
	inline VFXBinderBase_t8BC197F500B3A144DF76E77C48FA239C04CDA19C** GetAddressAtUnchecked(il2cpp_array_size_t index)
	{
		return m_Items + index;
	}
	inline void SetAtUnchecked(il2cpp_array_size_t index, VFXBinderBase_t8BC197F500B3A144DF76E77C48FA239C04CDA19C* value)
	{
		m_Items[index] = value;
		Il2CppCodeGenWriteBarrier((void**)m_Items + index, (void*)value);
	}
};
struct StringU5BU5D_t7674CD946EC0CE7B3AE0BE70E6EE85F2ECD9F248  : public RuntimeArray
{
	ALIGN_FIELD (8) String_t* m_Items[1];

	inline String_t* GetAt(il2cpp_array_size_t index) const
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		return m_Items[index];
	}
	inline String_t** GetAddressAt(il2cpp_array_size_t index)
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		return m_Items + index;
	}
	inline void SetAt(il2cpp_array_size_t index, String_t* value)
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		m_Items[index] = value;
		Il2CppCodeGenWriteBarrier((void**)m_Items + index, (void*)value);
	}
	inline String_t* GetAtUnchecked(il2cpp_array_size_t index) const
	{
		return m_Items[index];
	}
	inline String_t** GetAddressAtUnchecked(il2cpp_array_size_t index)
	{
		return m_Items + index;
	}
	inline void SetAtUnchecked(il2cpp_array_size_t index, String_t* value)
	{
		m_Items[index] = value;
		Il2CppCodeGenWriteBarrier((void**)m_Items + index, (void*)value);
	}
};
struct Vector3U5BU5D_tFF1859CCE176131B909E2044F76443064254679C  : public RuntimeArray
{
	ALIGN_FIELD (8) Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 m_Items[1];

	inline Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 GetAt(il2cpp_array_size_t index) const
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		return m_Items[index];
	}
	inline Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2* GetAddressAt(il2cpp_array_size_t index)
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		return m_Items + index;
	}
	inline void SetAt(il2cpp_array_size_t index, Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 value)
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		m_Items[index] = value;
	}
	inline Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 GetAtUnchecked(il2cpp_array_size_t index) const
	{
		return m_Items[index];
	}
	inline Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2* GetAddressAtUnchecked(il2cpp_array_size_t index)
	{
		return m_Items + index;
	}
	inline void SetAtUnchecked(il2cpp_array_size_t index, Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 value)
	{
		m_Items[index] = value;
	}
};


IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void List_1__ctor_mC54E2BCBE43279A96FC082F5CDE2D76388BD8F9C_gshared (List_1_t77B94703E05C519A9010DD0614F757F974E1CD8B* __this, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void List_1_Add_m79E50C4F592B1703F4B76A8BE7B4855515460CA1_gshared_inline (List_1_t77B94703E05C519A9010DD0614F757F974E1CD8B* __this, Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 ___0_item, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR int32_t List_1_get_Count_m46EEFFA770BE665EA0CB3A5332E941DA4B3C1D37_gshared_inline (List_1_t77B94703E05C519A9010DD0614F757F974E1CD8B* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void List_1__ctor_mE1298EFE02E4CF09A9513D04C6D0FE12C4A5A96D_gshared (List_1_t242CDEAEC9C92000DA96982CDB9D592DDE2AADAF* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Enumerator_t609488A9291E792D6A59707FF2BF03FBB727E047 List_1_GetEnumerator_m24A1CBC9428047F6672EC9B92A3E4154DC60337A_gshared (List_1_t77B94703E05C519A9010DD0614F757F974E1CD8B* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Enumerator_Dispose_m13D8B7747BCE3651CB2FD43622DBEDC3E50EA0E6_gshared (Enumerator_t609488A9291E792D6A59707FF2BF03FBB727E047* __this, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 Enumerator_get_Current_m098C9AE095DEFD00B1B0314A772330F537805EE2_gshared_inline (Enumerator_t609488A9291E792D6A59707FF2BF03FBB727E047* __this, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void List_1_Add_m4EA3769821DBDED34CA0EBACD784AC6C1A536C1A_gshared_inline (List_1_t242CDEAEC9C92000DA96982CDB9D592DDE2AADAF* __this, Color_tD001788D726C3A7F1379BEED0260B9591F440C1F ___0_item, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool Enumerator_MoveNext_m9C3D158CB944A2244D518FA7989D8312ACCD7C8A_gshared (Enumerator_t609488A9291E792D6A59707FF2BF03FBB727E047* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR ColorU5BU5D_t612261CF293F6FFC3D80AB52259FF0DC2B2CC389* List_1_ToArray_mD1EFBC7A0C9E4DF640DF58EC90D5494500C0571F_gshared (List_1_t242CDEAEC9C92000DA96982CDB9D592DDE2AADAF* __this, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* Array_Empty_TisRuntimeObject_mFB8A63D602BB6974D31E20300D9EB89C6FE7C278_gshared_inline (const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR RuntimeObject* Component_GetComponent_TisRuntimeObject_m7181F81CAEC2CF53F5D2BC79B7425C16E1F80D33_gshared (Component_t39FBE53E5EFCF4409111FB22C15FF73717632EC3* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool List_1_Contains_m4C9139C2A6B23E9343D3F87807B32C6E2CFE660D_gshared (List_1_tA239CB83DE5615F348BB0507E45F490F4F7C9A8D* __this, RuntimeObject* ___0_item, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void List_1_Add_mEBCF994CC3814631017F46A387B1A192ED6C85C7_gshared_inline (List_1_tA239CB83DE5615F348BB0507E45F490F4F7C9A8D* __this, RuntimeObject* ___0_item, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool List_1_Remove_m4DFA48F4CEB9169601E75FC28517C5C06EFA5AD7_gshared (List_1_tA239CB83DE5615F348BB0507E45F490F4F7C9A8D* __this, RuntimeObject* ___0_item, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void List_1__ctor_m7F078BB342729BDF11327FD89D7872265328F690_gshared (List_1_tA239CB83DE5615F348BB0507E45F490F4F7C9A8D* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* GameObject_GetComponents_TisRuntimeObject_m1654344F5C897CA86F9AB51530972086BDB05948_gshared (GameObject_t76FEDD663AB33C991A9C9A23129337651094216F* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void List_1_AddRange_m1F76B300133150E6046C5FED00E88B5DE0A02E17_gshared (List_1_tA239CB83DE5615F348BB0507E45F490F4F7C9A8D* __this, RuntimeObject* ___0_collection, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR RuntimeObject* List_1_get_Item_m33561245D64798C2AB07584C0EC4F240E4839A38_gshared (List_1_tA239CB83DE5615F348BB0507E45F490F4F7C9A8D* __this, int32_t ___0_index, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR int32_t List_1_get_Count_m4407E4C389F22B8CEC282C15D56516658746C383_gshared_inline (List_1_tA239CB83DE5615F348BB0507E45F490F4F7C9A8D* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* Component_GetComponents_TisRuntimeObject_m35447B0B07F9C06A1AF846E482EC6FEB09D2436F_gshared (Component_t39FBE53E5EFCF4409111FB22C15FF73717632EC3* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_NO_INLINE IL2CPP_METHOD_ATTR void List_1_AddWithResize_mCDB189127FC2EE67B5EED9DE34F837DFC6EDC212_gshared (List_1_t77B94703E05C519A9010DD0614F757F974E1CD8B* __this, Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 ___0_item, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_NO_INLINE IL2CPP_METHOD_ATTR void List_1_AddWithResize_m7AF5556215BF2EC986B31780ACADE159F4DAC1C5_gshared (List_1_t242CDEAEC9C92000DA96982CDB9D592DDE2AADAF* __this, Color_tD001788D726C3A7F1379BEED0260B9591F440C1F ___0_item, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_NO_INLINE IL2CPP_METHOD_ATTR void List_1_AddWithResize_m79A9BF770BEF9C06BE40D5401E55E375F2726CC4_gshared (List_1_tA239CB83DE5615F348BB0507E45F490F4F7C9A8D* __this, RuntimeObject* ___0_item, const RuntimeMethod* method) ;

IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void U3CU3Ec__ctor_m9AB57595F56A5AD38DD8DDFF11254F497A21AF07 (U3CU3Ec_t32602C6EC431C90BF3D26B05836A825F00E5B9DD* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Object__ctor_mE837C6B9FA8C6D5D109F4B2EC885D79919AC0EA2 (RuntimeObject* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool InputControlExtensions_IsPressed_m27B19824E37A51A57A0D05D7EDE767653427DBAA (InputControl_t74F06B623518F992BF8E38656A5E0857169E3E2E* ___0_control, float ___1_buttonPressPoint, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR String_t* ExposedProperty_op_Explicit_m8F7C257466E30C5602C4F12AEA2588B7093424F3_inline (ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* ___0_parameter, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* ExposedProperty_op_Implicit_m9CEB3D09454745C8E450BAB7C266D15ED28B44FB (String_t* ___0_name, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool Object_op_Inequality_mD0BE578448EAA61948F25C32F8DD55AB1F778602 (Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C* ___0_x, Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C* ___1_y, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* VFXLightBinder_get_ColorProperty_m985EDD04353195BBD32B94DF66317A170C93CAFD (VFXLightBinder_t5FDD7FBBDBFDB8A2AFCAEC4B46460F37EE4A672A* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool VisualEffect_HasVector4_m9BD809A0E57D0083CC65C3F8240FF3A55F866811 (VisualEffect_t8CF4F0E13AEBF27224BB1323CC103666EDBBCFB0* __this, String_t* ___0_name, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* VFXLightBinder_get_BrightnessProperty_mF1E9B4E4B53714CAF652F56B563BFD1E2C794AD1 (VFXLightBinder_t5FDD7FBBDBFDB8A2AFCAEC4B46460F37EE4A672A* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool VisualEffect_HasFloat_m96F1E6C9BB23E20D1BB730D3356E3E794D410B25 (VisualEffect_t8CF4F0E13AEBF27224BB1323CC103666EDBBCFB0* __this, String_t* ___0_name, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* VFXLightBinder_get_RadiusProperty_m0373D3B6410CA134EB6A4C3F5B9B7538583D5A20 (VFXLightBinder_t5FDD7FBBDBFDB8A2AFCAEC4B46460F37EE4A672A* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Color_tD001788D726C3A7F1379BEED0260B9591F440C1F Light_get_color_mE7EB8F11BF394877B50A2F335627441889ADE536 (Light_t1E68479B7782AF2050FAA02A5DC612FD034F18F3* __this, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR Vector4_t58B63D32F48C0DBF50DE2C60794C4676C80EDBE3 Color_op_Implicit_m9B3228DAFA8DC57A75DE00CBBF13ED4F1E7B01FF_inline (Color_tD001788D726C3A7F1379BEED0260B9591F440C1F ___0_c, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void VisualEffect_SetVector4_m2689F7AA23636CA94447027AFE213A03F51B261C (VisualEffect_t8CF4F0E13AEBF27224BB1323CC103666EDBBCFB0* __this, String_t* ___0_name, Vector4_t58B63D32F48C0DBF50DE2C60794C4676C80EDBE3 ___1_v, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR float Light_get_intensity_m8FA28D515853068A93FA68B2148809BBEE4E710F (Light_t1E68479B7782AF2050FAA02A5DC612FD034F18F3* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void VisualEffect_SetFloat_mB59095AC406A373BC587551B5A4781719F608F51 (VisualEffect_t8CF4F0E13AEBF27224BB1323CC103666EDBBCFB0* __this, String_t* ___0_name, float ___1_f, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR float Light_get_range_m4156F07BA6CD289DA47080B590D632721D975A22 (Light_t1E68479B7782AF2050FAA02A5DC612FD034F18F3* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool Object_op_Equality_mB6120F782D83091EF56A198FCEBCF066DB4A9605 (Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C* ___0_x, Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C* ___1_y, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* Object_get_name_mAC2F6B897CF1303BA4249B4CB55271AFACBB6392 (Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* String_Format_mFB7DA489BD99F4670881FF50EC017BFB0A5C0987 (String_t* ___0_format, RuntimeObject* ___1_arg0, RuntimeObject* ___2_arg1, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void VFXBinderBase__ctor_m4C0835B8FFF0968BC5378A1D579ABD0168ECF7D3 (VFXBinderBase_t8BC197F500B3A144DF76E77C48FA239C04CDA19C* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void VFXBinderBase_OnEnable_mA026DDFCBD31D8800C70F6A291A8DB6387F2041E (VFXBinderBase_t8BC197F500B3A144DF76E77C48FA239C04CDA19C* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void VFXMultiplePositionBinder_UpdateTexture_m08801CB93727EA213155BAC676BE9437E489B5E3 (VFXMultiplePositionBinder_t1891AAD3420705E4BA559CFF54038E8F6B27947F* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t ExposedProperty_op_Implicit_mA6E7C425842E5FA0B783360A77558C2D45A1970F (ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* ___0_parameter, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool VisualEffect_HasTexture_m9EE102518FBB3B2ACB6F1DED92638F7FD95AE2FE (VisualEffect_t8CF4F0E13AEBF27224BB1323CC103666EDBBCFB0* __this, int32_t ___0_nameID, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool VisualEffect_HasInt_m1F5851C9BEBD2A887EFF69694465401BDCD94789 (VisualEffect_t8CF4F0E13AEBF27224BB1323CC103666EDBBCFB0* __this, int32_t ___0_nameID, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool Application_get_isEditor_mEAC51E3ACE6DCE438087FB14BD75A3C219D354D0 (const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void VisualEffect_SetTexture_m96338B6E07291B861827B204CAE5654386D91E2C (VisualEffect_t8CF4F0E13AEBF27224BB1323CC103666EDBBCFB0* __this, int32_t ___0_nameID, Texture_t791CBB51219779964E0E8A2ED7C1AA5F92A4A700* ___1_t, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void VisualEffect_SetInt_m7F486C2E01DCAACE8D0A43661C8750822CEBD720 (VisualEffect_t8CF4F0E13AEBF27224BB1323CC103666EDBBCFB0* __this, int32_t ___0_nameID, int32_t ___1_i, const RuntimeMethod* method) ;
inline void List_1__ctor_mC54E2BCBE43279A96FC082F5CDE2D76388BD8F9C (List_1_t77B94703E05C519A9010DD0614F757F974E1CD8B* __this, const RuntimeMethod* method)
{
	((  void (*) (List_1_t77B94703E05C519A9010DD0614F757F974E1CD8B*, const RuntimeMethod*))List_1__ctor_mC54E2BCBE43279A96FC082F5CDE2D76388BD8F9C_gshared)(__this, method);
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Transform_tB27202C6F4E36D225EE28A13E4D662BF99785DB1* GameObject_get_transform_m0BC10ADFA1632166AE5544BDF9038A2650C2AE56 (GameObject_t76FEDD663AB33C991A9C9A23129337651094216F* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 Transform_get_position_m69CD5FA214FDAE7BB701552943674846C220FDE1 (Transform_tB27202C6F4E36D225EE28A13E4D662BF99785DB1* __this, const RuntimeMethod* method) ;
inline void List_1_Add_m79E50C4F592B1703F4B76A8BE7B4855515460CA1_inline (List_1_t77B94703E05C519A9010DD0614F757F974E1CD8B* __this, Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 ___0_item, const RuntimeMethod* method)
{
	((  void (*) (List_1_t77B94703E05C519A9010DD0614F757F974E1CD8B*, Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2, const RuntimeMethod*))List_1_Add_m79E50C4F592B1703F4B76A8BE7B4855515460CA1_gshared_inline)(__this, ___0_item, method);
}
inline int32_t List_1_get_Count_m46EEFFA770BE665EA0CB3A5332E941DA4B3C1D37_inline (List_1_t77B94703E05C519A9010DD0614F757F974E1CD8B* __this, const RuntimeMethod* method)
{
	return ((  int32_t (*) (List_1_t77B94703E05C519A9010DD0614F757F974E1CD8B*, const RuntimeMethod*))List_1_get_Count_m46EEFFA770BE665EA0CB3A5332E941DA4B3C1D37_gshared_inline)(__this, method);
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Texture2D__ctor_mECF60A9EC0638EC353C02C8E99B6B465D23BE917 (Texture2D_tE6505BC111DD8A424A9DBE8E05D7D09E11FFFCF4* __this, int32_t ___0_width, int32_t ___1_height, int32_t ___2_textureFormat, bool ___3_mipChain, const RuntimeMethod* method) ;
inline void List_1__ctor_mE1298EFE02E4CF09A9513D04C6D0FE12C4A5A96D (List_1_t242CDEAEC9C92000DA96982CDB9D592DDE2AADAF* __this, const RuntimeMethod* method)
{
	((  void (*) (List_1_t242CDEAEC9C92000DA96982CDB9D592DDE2AADAF*, const RuntimeMethod*))List_1__ctor_mE1298EFE02E4CF09A9513D04C6D0FE12C4A5A96D_gshared)(__this, method);
}
inline Enumerator_t609488A9291E792D6A59707FF2BF03FBB727E047 List_1_GetEnumerator_m24A1CBC9428047F6672EC9B92A3E4154DC60337A (List_1_t77B94703E05C519A9010DD0614F757F974E1CD8B* __this, const RuntimeMethod* method)
{
	return ((  Enumerator_t609488A9291E792D6A59707FF2BF03FBB727E047 (*) (List_1_t77B94703E05C519A9010DD0614F757F974E1CD8B*, const RuntimeMethod*))List_1_GetEnumerator_m24A1CBC9428047F6672EC9B92A3E4154DC60337A_gshared)(__this, method);
}
inline void Enumerator_Dispose_m13D8B7747BCE3651CB2FD43622DBEDC3E50EA0E6 (Enumerator_t609488A9291E792D6A59707FF2BF03FBB727E047* __this, const RuntimeMethod* method)
{
	((  void (*) (Enumerator_t609488A9291E792D6A59707FF2BF03FBB727E047*, const RuntimeMethod*))Enumerator_Dispose_m13D8B7747BCE3651CB2FD43622DBEDC3E50EA0E6_gshared)(__this, method);
}
inline Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 Enumerator_get_Current_m098C9AE095DEFD00B1B0314A772330F537805EE2_inline (Enumerator_t609488A9291E792D6A59707FF2BF03FBB727E047* __this, const RuntimeMethod* method)
{
	return ((  Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 (*) (Enumerator_t609488A9291E792D6A59707FF2BF03FBB727E047*, const RuntimeMethod*))Enumerator_get_Current_m098C9AE095DEFD00B1B0314A772330F537805EE2_gshared_inline)(__this, method);
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void Color__ctor_mCD6889CDE39F18704CD6EA8E2EFBFA48BA3E13B0_inline (Color_tD001788D726C3A7F1379BEED0260B9591F440C1F* __this, float ___0_r, float ___1_g, float ___2_b, const RuntimeMethod* method) ;
inline void List_1_Add_m4EA3769821DBDED34CA0EBACD784AC6C1A536C1A_inline (List_1_t242CDEAEC9C92000DA96982CDB9D592DDE2AADAF* __this, Color_tD001788D726C3A7F1379BEED0260B9591F440C1F ___0_item, const RuntimeMethod* method)
{
	((  void (*) (List_1_t242CDEAEC9C92000DA96982CDB9D592DDE2AADAF*, Color_tD001788D726C3A7F1379BEED0260B9591F440C1F, const RuntimeMethod*))List_1_Add_m4EA3769821DBDED34CA0EBACD784AC6C1A536C1A_gshared_inline)(__this, ___0_item, method);
}
inline bool Enumerator_MoveNext_m9C3D158CB944A2244D518FA7989D8312ACCD7C8A (Enumerator_t609488A9291E792D6A59707FF2BF03FBB727E047* __this, const RuntimeMethod* method)
{
	return ((  bool (*) (Enumerator_t609488A9291E792D6A59707FF2BF03FBB727E047*, const RuntimeMethod*))Enumerator_MoveNext_m9C3D158CB944A2244D518FA7989D8312ACCD7C8A_gshared)(__this, method);
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR GameObject_t76FEDD663AB33C991A9C9A23129337651094216F* Component_get_gameObject_m57AEFBB14DB39EC476F740BA000E170355DE691B (Component_t39FBE53E5EFCF4409111FB22C15FF73717632EC3* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* String_Concat_m9E3155FB84015C823606188F53B47CB44C444991 (String_t* ___0_str0, String_t* ___1_str1, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Object_set_name_mC79E6DC8FFD72479C90F0C4CC7F42A0FEAF5AE47 (Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C* __this, String_t* ___0_value, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Texture_set_filterMode_mE423E58C0C16D059EA62BA87AD70F44AEA50CCC9 (Texture_t791CBB51219779964E0E8A2ED7C1AA5F92A4A700* __this, int32_t ___0_value, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Texture_set_wrapMode_m1F74A690E3883EC9C5C371D502D09642F15D0F7E (Texture_t791CBB51219779964E0E8A2ED7C1AA5F92A4A700* __this, int32_t ___0_value, const RuntimeMethod* method) ;
inline ColorU5BU5D_t612261CF293F6FFC3D80AB52259FF0DC2B2CC389* List_1_ToArray_mD1EFBC7A0C9E4DF640DF58EC90D5494500C0571F (List_1_t242CDEAEC9C92000DA96982CDB9D592DDE2AADAF* __this, const RuntimeMethod* method)
{
	return ((  ColorU5BU5D_t612261CF293F6FFC3D80AB52259FF0DC2B2CC389* (*) (List_1_t242CDEAEC9C92000DA96982CDB9D592DDE2AADAF*, const RuntimeMethod*))List_1_ToArray_mD1EFBC7A0C9E4DF640DF58EC90D5494500C0571F_gshared)(__this, method);
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Texture2D_SetPixels_mF7DFA9F9B6085F7185C69DF108908B4A7CA4918C (Texture2D_tE6505BC111DD8A424A9DBE8E05D7D09E11FFFCF4* __this, ColorU5BU5D_t612261CF293F6FFC3D80AB52259FF0DC2B2CC389* ___0_colors, int32_t ___1_miplevel, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Texture2D_Apply_mA014182C9EE0BBF6EEE3B286854F29E50EB972DC (Texture2D_tE6505BC111DD8A424A9DBE8E05D7D09E11FFFCF4* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* String_Format_mA8DBB4C2516B9723C5A41E6CB1E2FAF4BBE96DD8 (String_t* ___0_format, RuntimeObject* ___1_arg0, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void VFXPlaneBinder_UpdateSubProperties_m9AEB13B4B517C1DBFD1DDA34390F682959FD909A (VFXPlaneBinder_t52D64377F31144687E6984CB470F8F20DF50AB58* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* ExposedProperty_op_Addition_m2FBBFB0417DEA6D43A730115FA3B9F39A3892C93 (ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* ___0_self, ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* ___1_other, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool VisualEffect_HasVector3_mADAFDFDB7950BEF5AFFB2CBD59E620B79F5431C6 (VisualEffect_t8CF4F0E13AEBF27224BB1323CC103666EDBBCFB0* __this, int32_t ___0_nameID, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Transform_tB27202C6F4E36D225EE28A13E4D662BF99785DB1* Component_get_transform_m2919A1D81931E6932C7F06D4C2F0AB8DDA9A5371 (Component_t39FBE53E5EFCF4409111FB22C15FF73717632EC3* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void VFXSpaceableBinder_ApplySpacePositionNormal_m57A07B73ED57359C970485FF3EBA907964822089 (VFXSpaceableBinder_t0C24970B59F1F8E2DA67992F0E90DF951BF91706* __this, VisualEffect_t8CF4F0E13AEBF27224BB1323CC103666EDBBCFB0* ___0_component, ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* ___1_targetProperty, Transform_tB27202C6F4E36D225EE28A13E4D662BF99785DB1* ___2_sourceTransform, Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2* ___3_position, Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2* ___4_normal, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void VisualEffect_SetVector3_m9DEA0241E8C6768309AEDC1319F3BD84EAB8D246 (VisualEffect_t8CF4F0E13AEBF27224BB1323CC103666EDBBCFB0* __this, int32_t ___0_nameID, Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 ___1_v, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void VFXSpaceableBinder__ctor_m22AD01FED423E4B640D31DC8DC13AD85422E1A04 (VFXSpaceableBinder_t0C24970B59F1F8E2DA67992F0E90DF951BF91706* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 VFXSpaceableBinder_ApplySpacePosition_mA1B55C45A80F225F7FC4562E74E306E90F95D12D (VFXSpaceableBinder_t0C24970B59F1F8E2DA67992F0E90DF951BF91706* __this, VisualEffect_t8CF4F0E13AEBF27224BB1323CC103666EDBBCFB0* ___0_component, ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* ___1_targetProperty, Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 ___2_sourceWorldPosition, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 Vector3_get_zero_m0C1249C3F25B1C70EAD3CC8B31259975A457AE39_inline (const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void VFXRaycastBinder_UpdateSubProperties_m984FC69B108C955BA03E05B08DEC583AAFFDE01D (VFXRaycastBinder_tD681421294476C02004DF02A34F958796887ED17* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool VisualEffect_HasBool_m880EB204D4723EB3C06B2E34A52486E678A752AA (VisualEffect_t8CF4F0E13AEBF27224BB1323CC103666EDBBCFB0* __this, int32_t ___0_nameID, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 Transform_TransformDirection_m9BE1261DF2D48B7A4A27D31EE24D2D97F89E7757 (Transform_tB27202C6F4E36D225EE28A13E4D662BF99785DB1* __this, Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 ___0_direction, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void Ray__ctor_mE298992FD10A3894C38373198385F345C58BD64C_inline (Ray_t2B1742D7958DC05BDC3EFC7461D3593E1430DC00* __this, Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 ___0_origin, Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 ___1_direction, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t LayerMask_op_Implicit_m7F5A5B9D079281AC445ED39DEE1FCFA9D795810D (LayerMask_t97CB6BDADEDC3D6423C7BCFEA7F86DA2EC6241DB ___0_mask, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool Physics_Raycast_m34AC1210E893A9EF969BD2C7104B10BE5B580025 (Ray_t2B1742D7958DC05BDC3EFC7461D3593E1430DC00 ___0_ray, RaycastHit_t6F30BD0B38B56401CA833A1B87BD74F2ACD2F2B5* ___1_hitInfo, float ___2_maxDistance, int32_t ___3_layerMask, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 RaycastHit_get_point_m02B764612562AFE0F998CC7CFB2EEDE41BA47F39 (RaycastHit_t6F30BD0B38B56401CA833A1B87BD74F2ACD2F2B5* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 RaycastHit_get_normal_mD8741B70D2039C5CAFC4368D4CE59D89562040B5 (RaycastHit_t6F30BD0B38B56401CA833A1B87BD74F2ACD2F2B5* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* VFXRaycastBinder_get_TargetHit_mCCE5712B9F489C7E060B1E7753D3F441852E8F1F (VFXRaycastBinder_tD681421294476C02004DF02A34F958796887ED17* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void VisualEffect_SetBool_m2AE92C97A9290E260655D557C4E68B2947E72964 (VisualEffect_t8CF4F0E13AEBF27224BB1323CC103666EDBBCFB0* __this, String_t* ___0_name, bool ___1_b, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* String_Format_mA0534D6E2AE4D67A6BD8D45B3321323930EB930C (String_t* ___0_format, RuntimeObject* ___1_arg0, RuntimeObject* ___2_arg1, RuntimeObject* ___3_arg2, const RuntimeMethod* method) ;
inline ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* Array_Empty_TisRuntimeObject_mFB8A63D602BB6974D31E20300D9EB89C6FE7C278_inline (const RuntimeMethod* method)
{
	return ((  ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* (*) (const RuntimeMethod*))Array_Empty_TisRuntimeObject_mFB8A63D602BB6974D31E20300D9EB89C6FE7C278_gshared_inline)(method);
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* String_Format_m918500C1EFB475181349A79989BB79BB36102894 (String_t* ___0_format, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___1_args, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 Vector3_get_forward_mAA55A7034304DF8B2152EAD49AE779FC4CA2EB4A_inline (const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR LayerMask_t97CB6BDADEDC3D6423C7BCFEA7F86DA2EC6241DB LayerMask_op_Implicit_m01C8996A2CB2085328B9C33539C43139660D8222 (int32_t ___0_intVal, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR VisualEffectAsset_t0CD57FD9980CD68033BD96DA356697F92A69E688* VisualEffect_get_visualEffectAsset_m301FCE98B138CAB2E16B46CE365538ECA8AF5F00 (VisualEffect_t8CF4F0E13AEBF27224BB1323CC103666EDBBCFB0* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t VisualEffectAsset_GetExposedSpace_m74640B9B45BE16A265F05649FC731EB9E3C4BE5B (VisualEffectAsset_t0CD57FD9980CD68033BD96DA356697F92A69E688* __this, int32_t ___0_nameID, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t VFXSpaceableBinder_GetTargetSpace_m0137508DC0E097C46C7EAA1ECC89398947D10D58 (VFXSpaceableBinder_t0C24970B59F1F8E2DA67992F0E90DF951BF91706* __this, VisualEffect_t8CF4F0E13AEBF27224BB1323CC103666EDBBCFB0* ___0_component, ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* ___1_targetProperty, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 Transform_get_worldToLocalMatrix_mB633C122A01BCE8E51B10B8B8CB95F580750B3F1 (Transform_tB27202C6F4E36D225EE28A13E4D662BF99785DB1* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 Transform_get_localToWorldMatrix_m5D35188766856338DD21DE756F42277C21719E6D (Transform_tB27202C6F4E36D225EE28A13E4D662BF99785DB1* __this, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 Matrix4x4_op_Multiply_m75E91775655DCA8DFC8EDE0AB787285BB3935162_inline (Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 ___0_lhs, Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 ___1_rhs, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 Matrix4x4_GetPosition_m9C2F34474967E47C83886A2EC335DFE37B563E80_inline (Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6* __this, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 Vector3_get_up_m128AF3FDC820BF59D5DE86D973E7DE3F20C3AEBA_inline (const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 Matrix4x4_MultiplyVector_mFD12F86A473E90BBB0002149ABA3917B2A518937_inline (Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6* __this, Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 ___0_vector, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 Transform_get_up_mE47A9D9D96422224DD0539AA5524DA5440145BB2 (Transform_tB27202C6F4E36D225EE28A13E4D662BF99785DB1* __this, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 Matrix4x4_get_lossyScale_mFB3D4CF6408D710D607CA1D2AF94B2E4E0B57EB7_inline (Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 Transform_get_lossyScale_mFF740DA4BE1489C6882CD2F3A37B7321176E5D07 (Transform_tB27202C6F4E36D225EE28A13E4D662BF99785DB1* __this, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974 Matrix4x4_get_rotation_m7E2C29FCB2AAFAE4D7B4FBD3563E9EDB53F5A8BB_inline (Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6* __this, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 Quaternion_get_eulerAngles_m2DB5158B5C3A71FD60FC8A6EE43D3AAA1CFED122_inline (Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 Transform_get_eulerAngles_mCAAF48EFCF628F1ED91C2FFE75A4FD19C039DD6A (Transform_tB27202C6F4E36D225EE28A13E4D662BF99785DB1* __this, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 Matrix4x4_MultiplyPoint_m20E910B65693559BFDE99382472D8DD02C862E7E_inline (Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6* __this, Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 ___0_point, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void VFXSphereBinder_UpdateSubProperties_mEDEF8CCE8D1B0CDE488F0A113CCF9D3699B4C4A6 (VFXSphereBinder_t1EC83273D815BCA9C53CD4F7D03C8EAE35811624* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool VisualEffect_HasFloat_m9F9969BD960A358735387BB53762466E14D289C6 (VisualEffect_t8CF4F0E13AEBF27224BB1323CC103666EDBBCFB0* __this, int32_t ___0_nameID, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void VFXSpaceableBinder_ApplySpaceTS_m3C7E145FA7A6D8B485B8992B7992FA984D76977C (VFXSpaceableBinder_t0C24970B59F1F8E2DA67992F0E90DF951BF91706* __this, VisualEffect_t8CF4F0E13AEBF27224BB1323CC103666EDBBCFB0* ___0_component, ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* ___1_targetProperty, Transform_tB27202C6F4E36D225EE28A13E4D662BF99785DB1* ___2_sourceTransform, Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2* ___3_position, Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2* ___4_scale, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 SphereCollider_get_center_m122A197607CD350873539A0EEE3BA10E8BE1759E (SphereCollider_tBA111C542CE97F6873DE742757213D6265C7D275* __this, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 Vector3_op_Addition_m78C0EC70CB66E8DCAC225743D82B268DAEE92067_inline (Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 ___0_a, Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 ___1_b, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR float SphereCollider_get_radius_m1BB513491906E76A4F71929E3DB72A1542309697 (SphereCollider_tBA111C542CE97F6873DE742757213D6265C7D275* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR float VFXSphereBinder_GetSphereColliderScale_mFF7014EA91C2D8244BD6F925F0FC267C908696D1 (VFXSphereBinder_t1EC83273D815BCA9C53CD4F7D03C8EAE35811624* __this, Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 ___0_scale, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void VisualEffect_SetFloat_m0AF9AD3C2CB14F8004B9F3BE3ECCCA74E3315BA2 (VisualEffect_t8CF4F0E13AEBF27224BB1323CC103666EDBBCFB0* __this, int32_t ___0_nameID, float ___1_f, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR float Mathf_Max_mF5379E63D2BBAC76D090748695D833934F8AD051_inline (float ___0_a, float ___1_b, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void VFXTerrainBinder_UpdateSubProperties_m4BC6A8E8C029DD3BD310534ABC857E2EF6FEE282 (VFXTerrainBinder_tBB926D3EE00EA5123B213E37F559F7DDDBFC173F* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR TerrainData_t615A68EAC648066681875D47FC641496D12F2E24* Terrain_get_terrainData_m3B6C1D89471A4E1C60FC19C168DB37A011B924FD (Terrain_t7F309492F67238DBFBC4566F47385B2A665CF667* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Bounds_t367E830C64BBF235ED8C3B2F8CF6254FDCAD39C3 TerrainData_get_bounds_m9CE9B3BF067EA06A837AB98B5BC6C0C4669B5A32 (TerrainData_t615A68EAC648066681875D47FC641496D12F2E24* __this, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 Bounds_get_center_m5B05F81CB835EB6DD8628FDA24B638F477984DC3_inline (Bounds_t367E830C64BBF235ED8C3B2F8CF6254FDCAD39C3* __this, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 Bounds_get_size_m0699A53A55A78B3201D7270D6F338DFA91B6FAD4_inline (Bounds_t367E830C64BBF235ED8C3B2F8CF6254FDCAD39C3* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR RenderTexture_tBA90C4C3AD9EECCFDDCC632D97C29FAB80D60D27* TerrainData_get_heightmapTexture_m26CB863455F0048CEA74741C71BF3021576FF8CE (TerrainData_t615A68EAC648066681875D47FC641496D12F2E24* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 TerrainData_get_heightmapScale_m4B6AB6495384109BA54955CA52B883A118015188 (TerrainData_t615A68EAC648066681875D47FC641496D12F2E24* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void VFXTransformBinder_UpdateSubProperties_mBF83D3F5E36B080C6CA43DF75A6C51B7A8AE6530 (VFXTransformBinder_t2658F13D9C6472FD34C1B8276A298E2F71FE042A* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void VFXSpaceableBinder_ApplySpaceTRS_m385C4C16AB3E5A84044D1406394ABC9B1A9BB748 (VFXSpaceableBinder_t0C24970B59F1F8E2DA67992F0E90DF951BF91706* __this, VisualEffect_t8CF4F0E13AEBF27224BB1323CC103666EDBBCFB0* ___0_component, ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* ___1_targetProperty, Transform_tB27202C6F4E36D225EE28A13E4D662BF99785DB1* ___2_sourceTransform, Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2* ___3_position, Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2* ___4_eulerAngles, Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2* ___5_scale, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR int32_t Dropdown_get_value_m386913162D5E273B762657FE5156DC567602BC3C_inline (Dropdown_t54C0BDC1441E058BE37E796F68886671C270EF89* __this, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR bool Toggle_get_isOn_m89A609E936CD67F460E336CA8E03C4047BFB6619_inline (Toggle_tBF13F3EBA485E06826FD8A38F4B4C1380DF21A1F* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void VisualEffect_SetBool_m835964F6BF4751B1B517E03F3676552164E7927D (VisualEffect_t8CF4F0E13AEBF27224BB1323CC103666EDBBCFB0* __this, int32_t ___0_nameID, bool ___1_b, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR float Time_get_time_m3A271BB1B20041144AC5B7863B71AB1F0150374B (const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 Vector3_op_Subtraction_mE42023FF80067CB44A1D4A27EB7CF2B24CABB828_inline (Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 ___0_a, Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 ___1_b, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR float Vector3_SqrMagnitude_m5ED73273031577902F1BED4CA9C5A0E786A680D1_inline (Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 ___0_vector, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 Vector3_op_Division_mCC6BB24E372AB96B8380D1678446EF6A8BAE13BB_inline (Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 ___0_a, float ___1_d, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void PropertyAttribute__ctor_m19247686E165101F140615C7306DC2DA3953D97D (PropertyAttribute_t5E0CB5A6CDA6E24CBD4FF26DE3B0C29D8BB54BF0* __this, const RuntimeMethod* method) ;
inline VFXPropertyBinder_t761F7FEC5308F4D9ABF7F0A5A3FAAAA6FE043C0F* Component_GetComponent_TisVFXPropertyBinder_t761F7FEC5308F4D9ABF7F0A5A3FAAAA6FE043C0F_m483C444935A49F5B275955F30616CCD6440760CA (Component_t39FBE53E5EFCF4409111FB22C15FF73717632EC3* __this, const RuntimeMethod* method)
{
	return ((  VFXPropertyBinder_t761F7FEC5308F4D9ABF7F0A5A3FAAAA6FE043C0F* (*) (Component_t39FBE53E5EFCF4409111FB22C15FF73717632EC3*, const RuntimeMethod*))Component_GetComponent_TisRuntimeObject_m7181F81CAEC2CF53F5D2BC79B7425C16E1F80D33_gshared)(__this, method);
}
inline bool List_1_Contains_mEB538ED5525355D6EF6699D85A92560EDA91A11F (List_1_t5A6730315AA46B39985DF97B343C4E4ACEF77B42* __this, VFXBinderBase_t8BC197F500B3A144DF76E77C48FA239C04CDA19C* ___0_item, const RuntimeMethod* method)
{
	return ((  bool (*) (List_1_t5A6730315AA46B39985DF97B343C4E4ACEF77B42*, VFXBinderBase_t8BC197F500B3A144DF76E77C48FA239C04CDA19C*, const RuntimeMethod*))List_1_Contains_m4C9139C2A6B23E9343D3F87807B32C6E2CFE660D_gshared)(__this, ___0_item, method);
}
inline void List_1_Add_m795E062030C3253617A08E869C6D58DF7FEC6799_inline (List_1_t5A6730315AA46B39985DF97B343C4E4ACEF77B42* __this, VFXBinderBase_t8BC197F500B3A144DF76E77C48FA239C04CDA19C* ___0_item, const RuntimeMethod* method)
{
	((  void (*) (List_1_t5A6730315AA46B39985DF97B343C4E4ACEF77B42*, VFXBinderBase_t8BC197F500B3A144DF76E77C48FA239C04CDA19C*, const RuntimeMethod*))List_1_Add_mEBCF994CC3814631017F46A387B1A192ED6C85C7_gshared_inline)(__this, ___0_item, method);
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Object_set_hideFlags_mACB8BFC903FB3B01BBD427753E791BF28B5E33D4 (Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C* __this, int32_t ___0_value, const RuntimeMethod* method) ;
inline bool List_1_Remove_mB0623D1E0568D9930B5F47623B7220C2F47B6CB1 (List_1_t5A6730315AA46B39985DF97B343C4E4ACEF77B42* __this, VFXBinderBase_t8BC197F500B3A144DF76E77C48FA239C04CDA19C* ___0_item, const RuntimeMethod* method)
{
	return ((  bool (*) (List_1_t5A6730315AA46B39985DF97B343C4E4ACEF77B42*, VFXBinderBase_t8BC197F500B3A144DF76E77C48FA239C04CDA19C*, const RuntimeMethod*))List_1_Remove_m4DFA48F4CEB9169601E75FC28517C5C06EFA5AD7_gshared)(__this, ___0_item, method);
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Type_t* Object_GetType_mE10A8FC1E57F3DF29972CCBC026C2DC3942263B3 (RuntimeObject* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void MonoBehaviour__ctor_m592DB0105CA0BC97AA1C5F4AD27B12D68A3B7C1E (MonoBehaviour_t532A11E69716D348D8AA7F854AFCBFCB8AD17F71* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void VFXPropertyBinder_Reload_mC20E8C114F88956652BCFE1D1AB7701BA946CA4A (VFXPropertyBinder_t761F7FEC5308F4D9ABF7F0A5A3FAAAA6FE043C0F* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Object_Destroy_mE97D0A766419A81296E8D4E5C23D01D3FE91ACBB (Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C* ___0_obj, const RuntimeMethod* method) ;
inline VisualEffect_t8CF4F0E13AEBF27224BB1323CC103666EDBBCFB0* Component_GetComponent_TisVisualEffect_t8CF4F0E13AEBF27224BB1323CC103666EDBBCFB0_mC2DD444DE804211C8EADF53E224C7103F9A3CCD1 (Component_t39FBE53E5EFCF4409111FB22C15FF73717632EC3* __this, const RuntimeMethod* method)
{
	return ((  VisualEffect_t8CF4F0E13AEBF27224BB1323CC103666EDBBCFB0* (*) (Component_t39FBE53E5EFCF4409111FB22C15FF73717632EC3*, const RuntimeMethod*))Component_GetComponent_TisRuntimeObject_m7181F81CAEC2CF53F5D2BC79B7425C16E1F80D33_gshared)(__this, method);
}
inline void List_1__ctor_m18A0E45DF3874B841F030036ABD1CBDCF7D77555 (List_1_t5A6730315AA46B39985DF97B343C4E4ACEF77B42* __this, const RuntimeMethod* method)
{
	((  void (*) (List_1_t5A6730315AA46B39985DF97B343C4E4ACEF77B42*, const RuntimeMethod*))List_1__ctor_m7F078BB342729BDF11327FD89D7872265328F690_gshared)(__this, method);
}
inline VFXBinderBaseU5BU5D_t1ECF22FF6432C40931AF7B0DEC149B606A33E247* GameObject_GetComponents_TisVFXBinderBase_t8BC197F500B3A144DF76E77C48FA239C04CDA19C_m2849BF084DFC48ABA7745E3C7EC2249159F03323 (GameObject_t76FEDD663AB33C991A9C9A23129337651094216F* __this, const RuntimeMethod* method)
{
	return ((  VFXBinderBaseU5BU5D_t1ECF22FF6432C40931AF7B0DEC149B606A33E247* (*) (GameObject_t76FEDD663AB33C991A9C9A23129337651094216F*, const RuntimeMethod*))GameObject_GetComponents_TisRuntimeObject_m1654344F5C897CA86F9AB51530972086BDB05948_gshared)(__this, method);
}
inline void List_1_AddRange_mD0480735EA168951277C2C2EB3F55E76699E8795 (List_1_t5A6730315AA46B39985DF97B343C4E4ACEF77B42* __this, RuntimeObject* ___0_collection, const RuntimeMethod* method)
{
	((  void (*) (List_1_t5A6730315AA46B39985DF97B343C4E4ACEF77B42*, RuntimeObject*, const RuntimeMethod*))List_1_AddRange_m1F76B300133150E6046C5FED00E88B5DE0A02E17_gshared)(__this, ___0_collection, method);
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void VFXPropertyBinder_ClearPropertyBinders_m0D6B59D73389A5A83E46E87D05B24ADE479CF13F (VFXPropertyBinder_t761F7FEC5308F4D9ABF7F0A5A3FAAAA6FE043C0F* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool Application_get_isPlaying_m25B0ABDFEF54F5370CD3F263A813540843D00F34 (const RuntimeMethod* method) ;
inline VFXBinderBase_t8BC197F500B3A144DF76E77C48FA239C04CDA19C* List_1_get_Item_m33073C80B80A639ECF4F79B6BDBE709573170A4E (List_1_t5A6730315AA46B39985DF97B343C4E4ACEF77B42* __this, int32_t ___0_index, const RuntimeMethod* method)
{
	return ((  VFXBinderBase_t8BC197F500B3A144DF76E77C48FA239C04CDA19C* (*) (List_1_t5A6730315AA46B39985DF97B343C4E4ACEF77B42*, int32_t, const RuntimeMethod*))List_1_get_Item_m33561245D64798C2AB07584C0EC4F240E4839A38_gshared)(__this, ___0_index, method);
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Debug_LogWarning_m33EF1B897E0C7C6FF538989610BFAFFEF4628CA9 (RuntimeObject* ___0_message, const RuntimeMethod* method) ;
inline int32_t List_1_get_Count_mF8853CB1C215FFD0674D1337D23514D811BBF2B2_inline (List_1_t5A6730315AA46B39985DF97B343C4E4ACEF77B42* __this, const RuntimeMethod* method)
{
	return ((  int32_t (*) (List_1_t5A6730315AA46B39985DF97B343C4E4ACEF77B42*, const RuntimeMethod*))List_1_get_Count_m4407E4C389F22B8CEC282C15D56516658746C383_gshared_inline)(__this, method);
}
inline VFXBinderBaseU5BU5D_t1ECF22FF6432C40931AF7B0DEC149B606A33E247* Component_GetComponents_TisVFXBinderBase_t8BC197F500B3A144DF76E77C48FA239C04CDA19C_m53E17DF23C2BBA4AD8DE56C0C006D00288F75A05 (Component_t39FBE53E5EFCF4409111FB22C15FF73717632EC3* __this, const RuntimeMethod* method)
{
	return ((  VFXBinderBaseU5BU5D_t1ECF22FF6432C40931AF7B0DEC149B606A33E247* (*) (Component_t39FBE53E5EFCF4409111FB22C15FF73717632EC3*, const RuntimeMethod*))Component_GetComponents_TisRuntimeObject_m35447B0B07F9C06A1AF846E482EC6FEB09D2436F_gshared)(__this, method);
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void VFXPropertyBinder_SafeDestroy_m4BC4B6A481989E373832AB7449F231ACB640F1AA (Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C* ___0_toDelete, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void VFXPropertyBinder_RemovePropertyBinder_mBFC783DFBF28ED3BB9ACC607CECCF9EF4661E863 (VFXPropertyBinder_t761F7FEC5308F4D9ABF7F0A5A3FAAAA6FE043C0F* __this, VFXBinderBase_t8BC197F500B3A144DF76E77C48FA239C04CDA19C* ___0_binder, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void Vector3_Normalize_mC749B887A4C74BA0A2E13E6377F17CCAEB0AADA8_inline (Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 Matrix4x4_GetLossyScale_m3C19D2C6746BB211C8CB02478A60EB2D71D10FC7 (Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974 Matrix4x4_GetRotation_mA3388AAB0DC4960D4E93D945398430A5C62D4D6F (Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 Quaternion_Internal_ToEulerRad_mC5BD020889B5A4FB6894CFB69A5D4C07B321B919 (Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974* ___0_rotation, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 Vector3_op_Multiply_m87BA7C578F96C8E49BB07088DAAC4649F83B0353_inline (Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 ___0_a, float ___1_d, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 Quaternion_Internal_MakePositive_m73E2D01920CB0DFE661A55022C129E8617F0C9A8 (Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 ___0_euler, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR float Vector3_get_sqrMagnitude_m43C27DEC47C4811FB30AB474FF2131A963B66FC8_inline (Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2* __this, const RuntimeMethod* method) ;
inline void List_1_AddWithResize_mCDB189127FC2EE67B5EED9DE34F837DFC6EDC212 (List_1_t77B94703E05C519A9010DD0614F757F974E1CD8B* __this, Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 ___0_item, const RuntimeMethod* method)
{
	((  void (*) (List_1_t77B94703E05C519A9010DD0614F757F974E1CD8B*, Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2, const RuntimeMethod*))List_1_AddWithResize_mCDB189127FC2EE67B5EED9DE34F837DFC6EDC212_gshared)(__this, ___0_item, method);
}
inline void List_1_AddWithResize_m7AF5556215BF2EC986B31780ACADE159F4DAC1C5 (List_1_t242CDEAEC9C92000DA96982CDB9D592DDE2AADAF* __this, Color_tD001788D726C3A7F1379BEED0260B9591F440C1F ___0_item, const RuntimeMethod* method)
{
	((  void (*) (List_1_t242CDEAEC9C92000DA96982CDB9D592DDE2AADAF*, Color_tD001788D726C3A7F1379BEED0260B9591F440C1F, const RuntimeMethod*))List_1_AddWithResize_m7AF5556215BF2EC986B31780ACADE159F4DAC1C5_gshared)(__this, ___0_item, method);
}
inline void List_1_AddWithResize_m79A9BF770BEF9C06BE40D5401E55E375F2726CC4 (List_1_tA239CB83DE5615F348BB0507E45F490F4F7C9A8D* __this, RuntimeObject* ___0_item, const RuntimeMethod* method)
{
	((  void (*) (List_1_tA239CB83DE5615F348BB0507E45F490F4F7C9A8D*, RuntimeObject*, const RuntimeMethod*))List_1_AddWithResize_m79A9BF770BEF9C06BE40D5401E55E375F2726CC4_gshared)(__this, ___0_item, method);
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR float Vector3_get_magnitude_mF0D6017E90B345F1F52D1CC564C640F1A847AF2D_inline (Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2* __this, const RuntimeMethod* method) ;
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void U3CU3Ec__cctor_mCF1AC0A123D5214071C87B14B9BEFA9F3E165D06 (const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&U3CU3Ec_t32602C6EC431C90BF3D26B05836A825F00E5B9DD_il2cpp_TypeInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	{
		U3CU3Ec_t32602C6EC431C90BF3D26B05836A825F00E5B9DD* L_0 = (U3CU3Ec_t32602C6EC431C90BF3D26B05836A825F00E5B9DD*)il2cpp_codegen_object_new(U3CU3Ec_t32602C6EC431C90BF3D26B05836A825F00E5B9DD_il2cpp_TypeInfo_var);
		U3CU3Ec__ctor_m9AB57595F56A5AD38DD8DDFF11254F497A21AF07(L_0, NULL);
		((U3CU3Ec_t32602C6EC431C90BF3D26B05836A825F00E5B9DD_StaticFields*)il2cpp_codegen_static_fields_for(U3CU3Ec_t32602C6EC431C90BF3D26B05836A825F00E5B9DD_il2cpp_TypeInfo_var))->___U3CU3E9 = L_0;
		Il2CppCodeGenWriteBarrier((void**)(&((U3CU3Ec_t32602C6EC431C90BF3D26B05836A825F00E5B9DD_StaticFields*)il2cpp_codegen_static_fields_for(U3CU3Ec_t32602C6EC431C90BF3D26B05836A825F00E5B9DD_il2cpp_TypeInfo_var))->___U3CU3E9), (void*)L_0);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void U3CU3Ec__ctor_m9AB57595F56A5AD38DD8DDFF11254F497A21AF07 (U3CU3Ec_t32602C6EC431C90BF3D26B05836A825F00E5B9DD* __this, const RuntimeMethod* method) 
{
	{
		Object__ctor_mE837C6B9FA8C6D5D109F4B2EC885D79919AC0EA2(__this, NULL);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool U3CU3Ec_U3CGetTouchCountU3Eb__20_0_m1C7DAEF04F30D57567824E2E97A7DE426E36462F (U3CU3Ec_t32602C6EC431C90BF3D26B05836A825F00E5B9DD* __this, TouchControl_t3A64A795FF61A8B27E5EBD61632F23AFCC725079* ___0_t, const RuntimeMethod* method) 
{
	{
		TouchControl_t3A64A795FF61A8B27E5EBD61632F23AFCC725079* L_0 = ___0_t;
		bool L_1;
		L_1 = InputControlExtensions_IsPressed_m27B19824E37A51A57A0D05D7EDE767653427DBAA(L_0, (0.0f), NULL);
		return L_1;
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
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* VFXLightBinder_get_ColorProperty_m985EDD04353195BBD32B94DF66317A170C93CAFD (VFXLightBinder_t5FDD7FBBDBFDB8A2AFCAEC4B46460F37EE4A672A* __this, const RuntimeMethod* method) 
{
	{
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_0 = __this->___m_ColorProperty;
		String_t* L_1;
		L_1 = ExposedProperty_op_Explicit_m8F7C257466E30C5602C4F12AEA2588B7093424F3_inline(L_0, NULL);
		return L_1;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void VFXLightBinder_set_ColorProperty_m202E67AB1B8A2265BBB8D4C1DB75646977469158 (VFXLightBinder_t5FDD7FBBDBFDB8A2AFCAEC4B46460F37EE4A672A* __this, String_t* ___0_value, const RuntimeMethod* method) 
{
	{
		String_t* L_0 = ___0_value;
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_1;
		L_1 = ExposedProperty_op_Implicit_m9CEB3D09454745C8E450BAB7C266D15ED28B44FB(L_0, NULL);
		__this->___m_ColorProperty = L_1;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___m_ColorProperty), (void*)L_1);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* VFXLightBinder_get_BrightnessProperty_mF1E9B4E4B53714CAF652F56B563BFD1E2C794AD1 (VFXLightBinder_t5FDD7FBBDBFDB8A2AFCAEC4B46460F37EE4A672A* __this, const RuntimeMethod* method) 
{
	{
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_0 = __this->___m_BrightnessProperty;
		String_t* L_1;
		L_1 = ExposedProperty_op_Explicit_m8F7C257466E30C5602C4F12AEA2588B7093424F3_inline(L_0, NULL);
		return L_1;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void VFXLightBinder_set_BrightnessProperty_mBBC16547EA68167DA5D72CBE1D92A85D3F19C0D5 (VFXLightBinder_t5FDD7FBBDBFDB8A2AFCAEC4B46460F37EE4A672A* __this, String_t* ___0_value, const RuntimeMethod* method) 
{
	{
		String_t* L_0 = ___0_value;
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_1;
		L_1 = ExposedProperty_op_Implicit_m9CEB3D09454745C8E450BAB7C266D15ED28B44FB(L_0, NULL);
		__this->___m_ColorProperty = L_1;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___m_ColorProperty), (void*)L_1);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* VFXLightBinder_get_RadiusProperty_m0373D3B6410CA134EB6A4C3F5B9B7538583D5A20 (VFXLightBinder_t5FDD7FBBDBFDB8A2AFCAEC4B46460F37EE4A672A* __this, const RuntimeMethod* method) 
{
	{
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_0 = __this->___m_RadiusProperty;
		String_t* L_1;
		L_1 = ExposedProperty_op_Explicit_m8F7C257466E30C5602C4F12AEA2588B7093424F3_inline(L_0, NULL);
		return L_1;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void VFXLightBinder_set_RadiusProperty_m399CFB05F9BFE7B45B9B07E94F837B2DF77500E6 (VFXLightBinder_t5FDD7FBBDBFDB8A2AFCAEC4B46460F37EE4A672A* __this, String_t* ___0_value, const RuntimeMethod* method) 
{
	{
		String_t* L_0 = ___0_value;
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_1;
		L_1 = ExposedProperty_op_Implicit_m9CEB3D09454745C8E450BAB7C266D15ED28B44FB(L_0, NULL);
		__this->___m_RadiusProperty = L_1;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___m_RadiusProperty), (void*)L_1);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool VFXLightBinder_IsValid_m18DC2B463BD5382C3E278CDC87F54BAD0DB2D602 (VFXLightBinder_t5FDD7FBBDBFDB8A2AFCAEC4B46460F37EE4A672A* __this, VisualEffect_t8CF4F0E13AEBF27224BB1323CC103666EDBBCFB0* ___0_component, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C_il2cpp_TypeInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	{
		Light_t1E68479B7782AF2050FAA02A5DC612FD034F18F3* L_0 = __this->___Target;
		il2cpp_codegen_runtime_class_init_inline(Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C_il2cpp_TypeInfo_var);
		bool L_1;
		L_1 = Object_op_Inequality_mD0BE578448EAA61948F25C32F8DD55AB1F778602(L_0, (Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C*)NULL, NULL);
		if (!L_1)
		{
			goto IL_0051;
		}
	}
	{
		bool L_2 = __this->___BindColor;
		if (!L_2)
		{
			goto IL_0024;
		}
	}
	{
		VisualEffect_t8CF4F0E13AEBF27224BB1323CC103666EDBBCFB0* L_3 = ___0_component;
		String_t* L_4;
		L_4 = VFXLightBinder_get_ColorProperty_m985EDD04353195BBD32B94DF66317A170C93CAFD(__this, NULL);
		NullCheck(L_3);
		bool L_5;
		L_5 = VisualEffect_HasVector4_m9BD809A0E57D0083CC65C3F8240FF3A55F866811(L_3, L_4, NULL);
		if (!L_5)
		{
			goto IL_0051;
		}
	}

IL_0024:
	{
		bool L_6 = __this->___BindBrightness;
		if (!L_6)
		{
			goto IL_003a;
		}
	}
	{
		VisualEffect_t8CF4F0E13AEBF27224BB1323CC103666EDBBCFB0* L_7 = ___0_component;
		String_t* L_8;
		L_8 = VFXLightBinder_get_BrightnessProperty_mF1E9B4E4B53714CAF652F56B563BFD1E2C794AD1(__this, NULL);
		NullCheck(L_7);
		bool L_9;
		L_9 = VisualEffect_HasFloat_m96F1E6C9BB23E20D1BB730D3356E3E794D410B25(L_7, L_8, NULL);
		if (!L_9)
		{
			goto IL_0051;
		}
	}

IL_003a:
	{
		bool L_10 = __this->___BindRadius;
		if (!L_10)
		{
			goto IL_004f;
		}
	}
	{
		VisualEffect_t8CF4F0E13AEBF27224BB1323CC103666EDBBCFB0* L_11 = ___0_component;
		String_t* L_12;
		L_12 = VFXLightBinder_get_RadiusProperty_m0373D3B6410CA134EB6A4C3F5B9B7538583D5A20(__this, NULL);
		NullCheck(L_11);
		bool L_13;
		L_13 = VisualEffect_HasFloat_m96F1E6C9BB23E20D1BB730D3356E3E794D410B25(L_11, L_12, NULL);
		return L_13;
	}

IL_004f:
	{
		return (bool)1;
	}

IL_0051:
	{
		return (bool)0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void VFXLightBinder_UpdateBinding_m745CD70F467410D2FEEA727CF58580F4692B304F (VFXLightBinder_t5FDD7FBBDBFDB8A2AFCAEC4B46460F37EE4A672A* __this, VisualEffect_t8CF4F0E13AEBF27224BB1323CC103666EDBBCFB0* ___0_component, const RuntimeMethod* method) 
{
	{
		bool L_0 = __this->___BindColor;
		if (!L_0)
		{
			goto IL_0024;
		}
	}
	{
		VisualEffect_t8CF4F0E13AEBF27224BB1323CC103666EDBBCFB0* L_1 = ___0_component;
		String_t* L_2;
		L_2 = VFXLightBinder_get_ColorProperty_m985EDD04353195BBD32B94DF66317A170C93CAFD(__this, NULL);
		Light_t1E68479B7782AF2050FAA02A5DC612FD034F18F3* L_3 = __this->___Target;
		NullCheck(L_3);
		Color_tD001788D726C3A7F1379BEED0260B9591F440C1F L_4;
		L_4 = Light_get_color_mE7EB8F11BF394877B50A2F335627441889ADE536(L_3, NULL);
		Vector4_t58B63D32F48C0DBF50DE2C60794C4676C80EDBE3 L_5;
		L_5 = Color_op_Implicit_m9B3228DAFA8DC57A75DE00CBBF13ED4F1E7B01FF_inline(L_4, NULL);
		NullCheck(L_1);
		VisualEffect_SetVector4_m2689F7AA23636CA94447027AFE213A03F51B261C(L_1, L_2, L_5, NULL);
	}

IL_0024:
	{
		bool L_6 = __this->___BindBrightness;
		if (!L_6)
		{
			goto IL_0043;
		}
	}
	{
		VisualEffect_t8CF4F0E13AEBF27224BB1323CC103666EDBBCFB0* L_7 = ___0_component;
		String_t* L_8;
		L_8 = VFXLightBinder_get_BrightnessProperty_mF1E9B4E4B53714CAF652F56B563BFD1E2C794AD1(__this, NULL);
		Light_t1E68479B7782AF2050FAA02A5DC612FD034F18F3* L_9 = __this->___Target;
		NullCheck(L_9);
		float L_10;
		L_10 = Light_get_intensity_m8FA28D515853068A93FA68B2148809BBEE4E710F(L_9, NULL);
		NullCheck(L_7);
		VisualEffect_SetFloat_mB59095AC406A373BC587551B5A4781719F608F51(L_7, L_8, L_10, NULL);
	}

IL_0043:
	{
		bool L_11 = __this->___BindRadius;
		if (!L_11)
		{
			goto IL_0062;
		}
	}
	{
		VisualEffect_t8CF4F0E13AEBF27224BB1323CC103666EDBBCFB0* L_12 = ___0_component;
		String_t* L_13;
		L_13 = VFXLightBinder_get_RadiusProperty_m0373D3B6410CA134EB6A4C3F5B9B7538583D5A20(__this, NULL);
		Light_t1E68479B7782AF2050FAA02A5DC612FD034F18F3* L_14 = __this->___Target;
		NullCheck(L_14);
		float L_15;
		L_15 = Light_get_range_m4156F07BA6CD289DA47080B590D632721D975A22(L_14, NULL);
		NullCheck(L_12);
		VisualEffect_SetFloat_mB59095AC406A373BC587551B5A4781719F608F51(L_12, L_13, L_15, NULL);
	}

IL_0062:
	{
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* VFXLightBinder_ToString_mD8BC48524A247DF67DA7670B8D3E6857A70DEACB (VFXLightBinder_t5FDD7FBBDBFDB8A2AFCAEC4B46460F37EE4A672A* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteral05C2E121588AEA79FB5F07594533A82A5DE429D8);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteralE6971243F7EB358A7C6E0D382075D789ADC51C31);
		s_Il2CppMethodInitialized = true;
	}
	ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* G_B2_0 = NULL;
	String_t* G_B2_1 = NULL;
	ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* G_B1_0 = NULL;
	String_t* G_B1_1 = NULL;
	String_t* G_B3_0 = NULL;
	ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* G_B3_1 = NULL;
	String_t* G_B3_2 = NULL;
	{
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_0 = __this->___m_ColorProperty;
		Light_t1E68479B7782AF2050FAA02A5DC612FD034F18F3* L_1 = __this->___Target;
		il2cpp_codegen_runtime_class_init_inline(Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C_il2cpp_TypeInfo_var);
		bool L_2;
		L_2 = Object_op_Equality_mB6120F782D83091EF56A198FCEBCF066DB4A9605(L_1, (Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C*)NULL, NULL);
		if (L_2)
		{
			G_B2_0 = L_0;
			G_B2_1 = _stringLiteral05C2E121588AEA79FB5F07594533A82A5DE429D8;
			goto IL_0026;
		}
		G_B1_0 = L_0;
		G_B1_1 = _stringLiteral05C2E121588AEA79FB5F07594533A82A5DE429D8;
	}
	{
		Light_t1E68479B7782AF2050FAA02A5DC612FD034F18F3* L_3 = __this->___Target;
		NullCheck(L_3);
		String_t* L_4;
		L_4 = Object_get_name_mAC2F6B897CF1303BA4249B4CB55271AFACBB6392(L_3, NULL);
		G_B3_0 = L_4;
		G_B3_1 = G_B1_0;
		G_B3_2 = G_B1_1;
		goto IL_002b;
	}

IL_0026:
	{
		G_B3_0 = _stringLiteralE6971243F7EB358A7C6E0D382075D789ADC51C31;
		G_B3_1 = G_B2_0;
		G_B3_2 = G_B2_1;
	}

IL_002b:
	{
		String_t* L_5;
		L_5 = String_Format_mFB7DA489BD99F4670881FF50EC017BFB0A5C0987(G_B3_2, G_B3_1, G_B3_0, NULL);
		return L_5;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void VFXLightBinder__ctor_m526A3FA1F645F0A8EFC83A2B07184CD3D57661E2 (VFXLightBinder_t5FDD7FBBDBFDB8A2AFCAEC4B46460F37EE4A672A* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteral19B7D722FFCBB1EBCC95DE76FB16F022050F3CC8);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteral4242348F79D89713783C664DBCE2420B6B2900DE);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteral74B50038C7C7797E6099EF7EC0273AAC048D793D);
		s_Il2CppMethodInitialized = true;
	}
	{
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_0;
		L_0 = ExposedProperty_op_Implicit_m9CEB3D09454745C8E450BAB7C266D15ED28B44FB(_stringLiteral19B7D722FFCBB1EBCC95DE76FB16F022050F3CC8, NULL);
		__this->___m_ColorProperty = L_0;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___m_ColorProperty), (void*)L_0);
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_1;
		L_1 = ExposedProperty_op_Implicit_m9CEB3D09454745C8E450BAB7C266D15ED28B44FB(_stringLiteral74B50038C7C7797E6099EF7EC0273AAC048D793D, NULL);
		__this->___m_BrightnessProperty = L_1;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___m_BrightnessProperty), (void*)L_1);
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_2;
		L_2 = ExposedProperty_op_Implicit_m9CEB3D09454745C8E450BAB7C266D15ED28B44FB(_stringLiteral4242348F79D89713783C664DBCE2420B6B2900DE, NULL);
		__this->___m_RadiusProperty = L_2;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___m_RadiusProperty), (void*)L_2);
		__this->___BindColor = (bool)1;
		VFXBinderBase__ctor_m4C0835B8FFF0968BC5378A1D579ABD0168ECF7D3(__this, NULL);
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
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void VFXMultiplePositionBinder_OnEnable_m81DA3CE28B764E868656402A55399AA93A84E289 (VFXMultiplePositionBinder_t1891AAD3420705E4BA559CFF54038E8F6B27947F* __this, const RuntimeMethod* method) 
{
	{
		VFXBinderBase_OnEnable_mA026DDFCBD31D8800C70F6A291A8DB6387F2041E(__this, NULL);
		VFXMultiplePositionBinder_UpdateTexture_m08801CB93727EA213155BAC676BE9437E489B5E3(__this, NULL);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool VFXMultiplePositionBinder_IsValid_m6B2D25A10A35F604BB649B554D00FEAB93ECED44 (VFXMultiplePositionBinder_t1891AAD3420705E4BA559CFF54038E8F6B27947F* __this, VisualEffect_t8CF4F0E13AEBF27224BB1323CC103666EDBBCFB0* ___0_component, const RuntimeMethod* method) 
{
	{
		GameObjectU5BU5D_tFF67550DFCE87096D7A3734EA15B75896B2722CF* L_0 = __this->___Targets;
		if (!L_0)
		{
			goto IL_002d;
		}
	}
	{
		VisualEffect_t8CF4F0E13AEBF27224BB1323CC103666EDBBCFB0* L_1 = ___0_component;
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_2 = __this->___PositionMapProperty;
		int32_t L_3;
		L_3 = ExposedProperty_op_Implicit_mA6E7C425842E5FA0B783360A77558C2D45A1970F(L_2, NULL);
		NullCheck(L_1);
		bool L_4;
		L_4 = VisualEffect_HasTexture_m9EE102518FBB3B2ACB6F1DED92638F7FD95AE2FE(L_1, L_3, NULL);
		if (!L_4)
		{
			goto IL_002d;
		}
	}
	{
		VisualEffect_t8CF4F0E13AEBF27224BB1323CC103666EDBBCFB0* L_5 = ___0_component;
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_6 = __this->___PositionCountProperty;
		int32_t L_7;
		L_7 = ExposedProperty_op_Implicit_mA6E7C425842E5FA0B783360A77558C2D45A1970F(L_6, NULL);
		NullCheck(L_5);
		bool L_8;
		L_8 = VisualEffect_HasInt_m1F5851C9BEBD2A887EFF69694465401BDCD94789(L_5, L_7, NULL);
		return L_8;
	}

IL_002d:
	{
		return (bool)0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void VFXMultiplePositionBinder_UpdateBinding_m4D1544BAB488ECC4ADDA4B5914AC1793C683147D (VFXMultiplePositionBinder_t1891AAD3420705E4BA559CFF54038E8F6B27947F* __this, VisualEffect_t8CF4F0E13AEBF27224BB1323CC103666EDBBCFB0* ___0_component, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Application_tDB03BE91CDF0ACA614A5E0B67CFB77C44EB19B21_il2cpp_TypeInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	{
		bool L_0 = __this->___EveryFrame;
		if (L_0)
		{
			goto IL_000f;
		}
	}
	{
		il2cpp_codegen_runtime_class_init_inline(Application_tDB03BE91CDF0ACA614A5E0B67CFB77C44EB19B21_il2cpp_TypeInfo_var);
		bool L_1;
		L_1 = Application_get_isEditor_mEAC51E3ACE6DCE438087FB14BD75A3C219D354D0(NULL);
		if (!L_1)
		{
			goto IL_0015;
		}
	}

IL_000f:
	{
		VFXMultiplePositionBinder_UpdateTexture_m08801CB93727EA213155BAC676BE9437E489B5E3(__this, NULL);
	}

IL_0015:
	{
		VisualEffect_t8CF4F0E13AEBF27224BB1323CC103666EDBBCFB0* L_2 = ___0_component;
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_3 = __this->___PositionMapProperty;
		int32_t L_4;
		L_4 = ExposedProperty_op_Implicit_mA6E7C425842E5FA0B783360A77558C2D45A1970F(L_3, NULL);
		Texture2D_tE6505BC111DD8A424A9DBE8E05D7D09E11FFFCF4* L_5 = __this->___positionMap;
		NullCheck(L_2);
		VisualEffect_SetTexture_m96338B6E07291B861827B204CAE5654386D91E2C(L_2, L_4, L_5, NULL);
		VisualEffect_t8CF4F0E13AEBF27224BB1323CC103666EDBBCFB0* L_6 = ___0_component;
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_7 = __this->___PositionCountProperty;
		int32_t L_8;
		L_8 = ExposedProperty_op_Implicit_mA6E7C425842E5FA0B783360A77558C2D45A1970F(L_7, NULL);
		int32_t L_9 = __this->___count;
		NullCheck(L_6);
		VisualEffect_SetInt_m7F486C2E01DCAACE8D0A43661C8750822CEBD720(L_6, L_8, L_9, NULL);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void VFXMultiplePositionBinder_UpdateTexture_m08801CB93727EA213155BAC676BE9437E489B5E3 (VFXMultiplePositionBinder_t1891AAD3420705E4BA559CFF54038E8F6B27947F* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Enumerator_Dispose_m13D8B7747BCE3651CB2FD43622DBEDC3E50EA0E6_RuntimeMethod_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Enumerator_MoveNext_m9C3D158CB944A2244D518FA7989D8312ACCD7C8A_RuntimeMethod_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Enumerator_get_Current_m098C9AE095DEFD00B1B0314A772330F537805EE2_RuntimeMethod_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&List_1_Add_m4EA3769821DBDED34CA0EBACD784AC6C1A536C1A_RuntimeMethod_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&List_1_Add_m79E50C4F592B1703F4B76A8BE7B4855515460CA1_RuntimeMethod_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&List_1_GetEnumerator_m24A1CBC9428047F6672EC9B92A3E4154DC60337A_RuntimeMethod_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&List_1_ToArray_mD1EFBC7A0C9E4DF640DF58EC90D5494500C0571F_RuntimeMethod_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&List_1__ctor_mC54E2BCBE43279A96FC082F5CDE2D76388BD8F9C_RuntimeMethod_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&List_1__ctor_mE1298EFE02E4CF09A9513D04C6D0FE12C4A5A96D_RuntimeMethod_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&List_1_get_Count_m46EEFFA770BE665EA0CB3A5332E941DA4B3C1D37_RuntimeMethod_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&List_1_t242CDEAEC9C92000DA96982CDB9D592DDE2AADAF_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&List_1_t77B94703E05C519A9010DD0614F757F974E1CD8B_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Texture2D_tE6505BC111DD8A424A9DBE8E05D7D09E11FFFCF4_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteral8E949677B6004F3DD1FFF60DF500D893920D80EA);
		s_Il2CppMethodInitialized = true;
	}
	List_1_t77B94703E05C519A9010DD0614F757F974E1CD8B* V_0 = NULL;
	List_1_t242CDEAEC9C92000DA96982CDB9D592DDE2AADAF* V_1 = NULL;
	GameObjectU5BU5D_tFF67550DFCE87096D7A3734EA15B75896B2722CF* V_2 = NULL;
	int32_t V_3 = 0;
	GameObject_t76FEDD663AB33C991A9C9A23129337651094216F* V_4 = NULL;
	Enumerator_t609488A9291E792D6A59707FF2BF03FBB727E047 V_5;
	memset((&V_5), 0, sizeof(V_5));
	Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 V_6;
	memset((&V_6), 0, sizeof(V_6));
	{
		GameObjectU5BU5D_tFF67550DFCE87096D7A3734EA15B75896B2722CF* L_0 = __this->___Targets;
		if (!L_0)
		{
			goto IL_0011;
		}
	}
	{
		GameObjectU5BU5D_tFF67550DFCE87096D7A3734EA15B75896B2722CF* L_1 = __this->___Targets;
		NullCheck(L_1);
		if ((((RuntimeArray*)L_1)->max_length))
		{
			goto IL_0012;
		}
	}

IL_0011:
	{
		return;
	}

IL_0012:
	{
		List_1_t77B94703E05C519A9010DD0614F757F974E1CD8B* L_2 = (List_1_t77B94703E05C519A9010DD0614F757F974E1CD8B*)il2cpp_codegen_object_new(List_1_t77B94703E05C519A9010DD0614F757F974E1CD8B_il2cpp_TypeInfo_var);
		List_1__ctor_mC54E2BCBE43279A96FC082F5CDE2D76388BD8F9C(L_2, List_1__ctor_mC54E2BCBE43279A96FC082F5CDE2D76388BD8F9C_RuntimeMethod_var);
		V_0 = L_2;
		GameObjectU5BU5D_tFF67550DFCE87096D7A3734EA15B75896B2722CF* L_3 = __this->___Targets;
		V_2 = L_3;
		V_3 = 0;
		goto IL_0048;
	}

IL_0023:
	{
		GameObjectU5BU5D_tFF67550DFCE87096D7A3734EA15B75896B2722CF* L_4 = V_2;
		int32_t L_5 = V_3;
		NullCheck(L_4);
		int32_t L_6 = L_5;
		GameObject_t76FEDD663AB33C991A9C9A23129337651094216F* L_7 = (L_4)->GetAt(static_cast<il2cpp_array_size_t>(L_6));
		V_4 = L_7;
		GameObject_t76FEDD663AB33C991A9C9A23129337651094216F* L_8 = V_4;
		il2cpp_codegen_runtime_class_init_inline(Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C_il2cpp_TypeInfo_var);
		bool L_9;
		L_9 = Object_op_Inequality_mD0BE578448EAA61948F25C32F8DD55AB1F778602(L_8, (Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C*)NULL, NULL);
		if (!L_9)
		{
			goto IL_0044;
		}
	}
	{
		List_1_t77B94703E05C519A9010DD0614F757F974E1CD8B* L_10 = V_0;
		GameObject_t76FEDD663AB33C991A9C9A23129337651094216F* L_11 = V_4;
		NullCheck(L_11);
		Transform_tB27202C6F4E36D225EE28A13E4D662BF99785DB1* L_12;
		L_12 = GameObject_get_transform_m0BC10ADFA1632166AE5544BDF9038A2650C2AE56(L_11, NULL);
		NullCheck(L_12);
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_13;
		L_13 = Transform_get_position_m69CD5FA214FDAE7BB701552943674846C220FDE1(L_12, NULL);
		NullCheck(L_10);
		List_1_Add_m79E50C4F592B1703F4B76A8BE7B4855515460CA1_inline(L_10, L_13, List_1_Add_m79E50C4F592B1703F4B76A8BE7B4855515460CA1_RuntimeMethod_var);
	}

IL_0044:
	{
		int32_t L_14 = V_3;
		V_3 = ((int32_t)il2cpp_codegen_add(L_14, 1));
	}

IL_0048:
	{
		int32_t L_15 = V_3;
		GameObjectU5BU5D_tFF67550DFCE87096D7A3734EA15B75896B2722CF* L_16 = V_2;
		NullCheck(L_16);
		if ((((int32_t)L_15) < ((int32_t)((int32_t)(((RuntimeArray*)L_16)->max_length)))))
		{
			goto IL_0023;
		}
	}
	{
		List_1_t77B94703E05C519A9010DD0614F757F974E1CD8B* L_17 = V_0;
		NullCheck(L_17);
		int32_t L_18;
		L_18 = List_1_get_Count_m46EEFFA770BE665EA0CB3A5332E941DA4B3C1D37_inline(L_17, List_1_get_Count_m46EEFFA770BE665EA0CB3A5332E941DA4B3C1D37_RuntimeMethod_var);
		__this->___count = L_18;
		Texture2D_tE6505BC111DD8A424A9DBE8E05D7D09E11FFFCF4* L_19 = __this->___positionMap;
		il2cpp_codegen_runtime_class_init_inline(Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C_il2cpp_TypeInfo_var);
		bool L_20;
		L_20 = Object_op_Equality_mB6120F782D83091EF56A198FCEBCF066DB4A9605(L_19, (Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C*)NULL, NULL);
		if (L_20)
		{
			goto IL_007b;
		}
	}
	{
		Texture2D_tE6505BC111DD8A424A9DBE8E05D7D09E11FFFCF4* L_21 = __this->___positionMap;
		NullCheck(L_21);
		int32_t L_22;
		L_22 = VirtualFuncInvoker0< int32_t >::Invoke(5, L_21);
		int32_t L_23 = __this->___count;
		if ((((int32_t)L_22) == ((int32_t)L_23)))
		{
			goto IL_0090;
		}
	}

IL_007b:
	{
		int32_t L_24 = __this->___count;
		Texture2D_tE6505BC111DD8A424A9DBE8E05D7D09E11FFFCF4* L_25 = (Texture2D_tE6505BC111DD8A424A9DBE8E05D7D09E11FFFCF4*)il2cpp_codegen_object_new(Texture2D_tE6505BC111DD8A424A9DBE8E05D7D09E11FFFCF4_il2cpp_TypeInfo_var);
		Texture2D__ctor_mECF60A9EC0638EC353C02C8E99B6B465D23BE917(L_25, L_24, 1, ((int32_t)20), (bool)0, NULL);
		__this->___positionMap = L_25;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___positionMap), (void*)L_25);
	}

IL_0090:
	{
		List_1_t242CDEAEC9C92000DA96982CDB9D592DDE2AADAF* L_26 = (List_1_t242CDEAEC9C92000DA96982CDB9D592DDE2AADAF*)il2cpp_codegen_object_new(List_1_t242CDEAEC9C92000DA96982CDB9D592DDE2AADAF_il2cpp_TypeInfo_var);
		List_1__ctor_mE1298EFE02E4CF09A9513D04C6D0FE12C4A5A96D(L_26, List_1__ctor_mE1298EFE02E4CF09A9513D04C6D0FE12C4A5A96D_RuntimeMethod_var);
		V_1 = L_26;
		List_1_t77B94703E05C519A9010DD0614F757F974E1CD8B* L_27 = V_0;
		NullCheck(L_27);
		Enumerator_t609488A9291E792D6A59707FF2BF03FBB727E047 L_28;
		L_28 = List_1_GetEnumerator_m24A1CBC9428047F6672EC9B92A3E4154DC60337A(L_27, List_1_GetEnumerator_m24A1CBC9428047F6672EC9B92A3E4154DC60337A_RuntimeMethod_var);
		V_5 = L_28;
	}
	{
		auto __finallyBlock = il2cpp::utils::Finally([&]
		{

FINALLY_00d4:
			{
				Enumerator_Dispose_m13D8B7747BCE3651CB2FD43622DBEDC3E50EA0E6((&V_5), Enumerator_Dispose_m13D8B7747BCE3651CB2FD43622DBEDC3E50EA0E6_RuntimeMethod_var);
				return;
			}
		});
		try
		{
			{
				goto IL_00c9_1;
			}

IL_00a0_1:
			{
				Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_29;
				L_29 = Enumerator_get_Current_m098C9AE095DEFD00B1B0314A772330F537805EE2_inline((&V_5), Enumerator_get_Current_m098C9AE095DEFD00B1B0314A772330F537805EE2_RuntimeMethod_var);
				V_6 = L_29;
				List_1_t242CDEAEC9C92000DA96982CDB9D592DDE2AADAF* L_30 = V_1;
				Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_31 = V_6;
				float L_32 = L_31.___x;
				Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_33 = V_6;
				float L_34 = L_33.___y;
				Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_35 = V_6;
				float L_36 = L_35.___z;
				Color_tD001788D726C3A7F1379BEED0260B9591F440C1F L_37;
				memset((&L_37), 0, sizeof(L_37));
				Color__ctor_mCD6889CDE39F18704CD6EA8E2EFBFA48BA3E13B0_inline((&L_37), L_32, L_34, L_36, NULL);
				NullCheck(L_30);
				List_1_Add_m4EA3769821DBDED34CA0EBACD784AC6C1A536C1A_inline(L_30, L_37, List_1_Add_m4EA3769821DBDED34CA0EBACD784AC6C1A536C1A_RuntimeMethod_var);
			}

IL_00c9_1:
			{
				bool L_38;
				L_38 = Enumerator_MoveNext_m9C3D158CB944A2244D518FA7989D8312ACCD7C8A((&V_5), Enumerator_MoveNext_m9C3D158CB944A2244D518FA7989D8312ACCD7C8A_RuntimeMethod_var);
				if (L_38)
				{
					goto IL_00a0_1;
				}
			}
			{
				goto IL_00e2;
			}
		}
		catch(Il2CppExceptionWrapper& e)
		{
			__finallyBlock.StoreException(e.ex);
		}
	}

IL_00e2:
	{
		Texture2D_tE6505BC111DD8A424A9DBE8E05D7D09E11FFFCF4* L_39 = __this->___positionMap;
		GameObject_t76FEDD663AB33C991A9C9A23129337651094216F* L_40;
		L_40 = Component_get_gameObject_m57AEFBB14DB39EC476F740BA000E170355DE691B(__this, NULL);
		NullCheck(L_40);
		String_t* L_41;
		L_41 = Object_get_name_mAC2F6B897CF1303BA4249B4CB55271AFACBB6392(L_40, NULL);
		String_t* L_42;
		L_42 = String_Concat_m9E3155FB84015C823606188F53B47CB44C444991(L_41, _stringLiteral8E949677B6004F3DD1FFF60DF500D893920D80EA, NULL);
		NullCheck(L_39);
		Object_set_name_mC79E6DC8FFD72479C90F0C4CC7F42A0FEAF5AE47(L_39, L_42, NULL);
		Texture2D_tE6505BC111DD8A424A9DBE8E05D7D09E11FFFCF4* L_43 = __this->___positionMap;
		NullCheck(L_43);
		Texture_set_filterMode_mE423E58C0C16D059EA62BA87AD70F44AEA50CCC9(L_43, 0, NULL);
		Texture2D_tE6505BC111DD8A424A9DBE8E05D7D09E11FFFCF4* L_44 = __this->___positionMap;
		NullCheck(L_44);
		Texture_set_wrapMode_m1F74A690E3883EC9C5C371D502D09642F15D0F7E(L_44, 0, NULL);
		Texture2D_tE6505BC111DD8A424A9DBE8E05D7D09E11FFFCF4* L_45 = __this->___positionMap;
		List_1_t242CDEAEC9C92000DA96982CDB9D592DDE2AADAF* L_46 = V_1;
		NullCheck(L_46);
		ColorU5BU5D_t612261CF293F6FFC3D80AB52259FF0DC2B2CC389* L_47;
		L_47 = List_1_ToArray_mD1EFBC7A0C9E4DF640DF58EC90D5494500C0571F(L_46, List_1_ToArray_mD1EFBC7A0C9E4DF640DF58EC90D5494500C0571F_RuntimeMethod_var);
		NullCheck(L_45);
		Texture2D_SetPixels_mF7DFA9F9B6085F7185C69DF108908B4A7CA4918C(L_45, L_47, 0, NULL);
		Texture2D_tE6505BC111DD8A424A9DBE8E05D7D09E11FFFCF4* L_48 = __this->___positionMap;
		NullCheck(L_48);
		Texture2D_Apply_mA014182C9EE0BBF6EEE3B286854F29E50EB972DC(L_48, NULL);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* VFXMultiplePositionBinder_ToString_mEDB13894DBEAF2F31994A72310A23231A6A7BDCA (VFXMultiplePositionBinder_t1891AAD3420705E4BA559CFF54038E8F6B27947F* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteralEC77BC114B027ACF6D87D758555A29750B472EBC);
		s_Il2CppMethodInitialized = true;
	}
	{
		int32_t L_0 = __this->___count;
		int32_t L_1 = L_0;
		RuntimeObject* L_2 = Box(il2cpp_defaults.int32_class, &L_1);
		String_t* L_3;
		L_3 = String_Format_mA8DBB4C2516B9723C5A41E6CB1E2FAF4BBE96DD8(_stringLiteralEC77BC114B027ACF6D87D758555A29750B472EBC, L_2, NULL);
		return L_3;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void VFXMultiplePositionBinder__ctor_mE710FDA9E568A663416CB22750A80995D7169FEF (VFXMultiplePositionBinder_t1891AAD3420705E4BA559CFF54038E8F6B27947F* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteralA7B1476A1E5CD3BB02076DE8E002321B1E3433D5);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteralDDD2EA3792482A37AF09C85C0BB5CA7E47820DC9);
		s_Il2CppMethodInitialized = true;
	}
	{
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_0;
		L_0 = ExposedProperty_op_Implicit_m9CEB3D09454745C8E450BAB7C266D15ED28B44FB(_stringLiteralA7B1476A1E5CD3BB02076DE8E002321B1E3433D5, NULL);
		__this->___PositionMapProperty = L_0;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___PositionMapProperty), (void*)L_0);
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_1;
		L_1 = ExposedProperty_op_Implicit_m9CEB3D09454745C8E450BAB7C266D15ED28B44FB(_stringLiteralDDD2EA3792482A37AF09C85C0BB5CA7E47820DC9, NULL);
		__this->___PositionCountProperty = L_1;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___PositionCountProperty), (void*)L_1);
		VFXBinderBase__ctor_m4C0835B8FFF0968BC5378A1D579ABD0168ECF7D3(__this, NULL);
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
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* VFXPlaneBinder_get_Property_m54C30C4D74B21178FCB92E920F2B0B004E611CB7 (VFXPlaneBinder_t52D64377F31144687E6984CB470F8F20DF50AB58* __this, const RuntimeMethod* method) 
{
	{
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_0 = __this->___m_Property;
		String_t* L_1;
		L_1 = ExposedProperty_op_Explicit_m8F7C257466E30C5602C4F12AEA2588B7093424F3_inline(L_0, NULL);
		return L_1;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void VFXPlaneBinder_set_Property_m91934DD97598848E257D6841DF8E6583AA71E57C (VFXPlaneBinder_t52D64377F31144687E6984CB470F8F20DF50AB58* __this, String_t* ___0_value, const RuntimeMethod* method) 
{
	{
		String_t* L_0 = ___0_value;
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_1;
		L_1 = ExposedProperty_op_Implicit_m9CEB3D09454745C8E450BAB7C266D15ED28B44FB(L_0, NULL);
		__this->___m_Property = L_1;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___m_Property), (void*)L_1);
		VFXPlaneBinder_UpdateSubProperties_m9AEB13B4B517C1DBFD1DDA34390F682959FD909A(__this, NULL);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void VFXPlaneBinder_OnEnable_m7367BBAC66F8ACCA856E6FE63D3F320C79C0C2BC (VFXPlaneBinder_t52D64377F31144687E6984CB470F8F20DF50AB58* __this, const RuntimeMethod* method) 
{
	{
		VFXBinderBase_OnEnable_mA026DDFCBD31D8800C70F6A291A8DB6387F2041E(__this, NULL);
		VFXPlaneBinder_UpdateSubProperties_m9AEB13B4B517C1DBFD1DDA34390F682959FD909A(__this, NULL);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void VFXPlaneBinder_OnValidate_m7950C4FB69814A74319778D4DECBB657C686E2FD (VFXPlaneBinder_t52D64377F31144687E6984CB470F8F20DF50AB58* __this, const RuntimeMethod* method) 
{
	{
		VFXPlaneBinder_UpdateSubProperties_m9AEB13B4B517C1DBFD1DDA34390F682959FD909A(__this, NULL);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void VFXPlaneBinder_UpdateSubProperties_m9AEB13B4B517C1DBFD1DDA34390F682959FD909A (VFXPlaneBinder_t52D64377F31144687E6984CB470F8F20DF50AB58* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteralCA4FE6DE4D088688B7375BECF5966DF1682F104D);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteralCD758053CBD26018308E17600F943A6C1A513D7E);
		s_Il2CppMethodInitialized = true;
	}
	{
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_0 = __this->___m_Property;
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_1;
		L_1 = ExposedProperty_op_Implicit_m9CEB3D09454745C8E450BAB7C266D15ED28B44FB(_stringLiteralCA4FE6DE4D088688B7375BECF5966DF1682F104D, NULL);
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_2;
		L_2 = ExposedProperty_op_Addition_m2FBBFB0417DEA6D43A730115FA3B9F39A3892C93(L_0, L_1, NULL);
		__this->___Position = L_2;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___Position), (void*)L_2);
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_3 = __this->___m_Property;
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_4;
		L_4 = ExposedProperty_op_Implicit_m9CEB3D09454745C8E450BAB7C266D15ED28B44FB(_stringLiteralCD758053CBD26018308E17600F943A6C1A513D7E, NULL);
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_5;
		L_5 = ExposedProperty_op_Addition_m2FBBFB0417DEA6D43A730115FA3B9F39A3892C93(L_3, L_4, NULL);
		__this->___Normal = L_5;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___Normal), (void*)L_5);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool VFXPlaneBinder_IsValid_m4F681A04FB92422F01268AE5E4D943A5BFB0B505 (VFXPlaneBinder_t52D64377F31144687E6984CB470F8F20DF50AB58* __this, VisualEffect_t8CF4F0E13AEBF27224BB1323CC103666EDBBCFB0* ___0_component, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C_il2cpp_TypeInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	{
		Transform_tB27202C6F4E36D225EE28A13E4D662BF99785DB1* L_0 = __this->___Target;
		il2cpp_codegen_runtime_class_init_inline(Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C_il2cpp_TypeInfo_var);
		bool L_1;
		L_1 = Object_op_Inequality_mD0BE578448EAA61948F25C32F8DD55AB1F778602(L_0, (Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C*)NULL, NULL);
		if (!L_1)
		{
			goto IL_0033;
		}
	}
	{
		VisualEffect_t8CF4F0E13AEBF27224BB1323CC103666EDBBCFB0* L_2 = ___0_component;
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_3 = __this->___Position;
		int32_t L_4;
		L_4 = ExposedProperty_op_Implicit_mA6E7C425842E5FA0B783360A77558C2D45A1970F(L_3, NULL);
		NullCheck(L_2);
		bool L_5;
		L_5 = VisualEffect_HasVector3_mADAFDFDB7950BEF5AFFB2CBD59E620B79F5431C6(L_2, L_4, NULL);
		if (!L_5)
		{
			goto IL_0033;
		}
	}
	{
		VisualEffect_t8CF4F0E13AEBF27224BB1323CC103666EDBBCFB0* L_6 = ___0_component;
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_7 = __this->___Normal;
		int32_t L_8;
		L_8 = ExposedProperty_op_Implicit_mA6E7C425842E5FA0B783360A77558C2D45A1970F(L_7, NULL);
		NullCheck(L_6);
		bool L_9;
		L_9 = VisualEffect_HasVector3_mADAFDFDB7950BEF5AFFB2CBD59E620B79F5431C6(L_6, L_8, NULL);
		return L_9;
	}

IL_0033:
	{
		return (bool)0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void VFXPlaneBinder_UpdateBinding_mE2DD9576ADF5DC76AEE00656785F77B05B2E6B1C (VFXPlaneBinder_t52D64377F31144687E6984CB470F8F20DF50AB58* __this, VisualEffect_t8CF4F0E13AEBF27224BB1323CC103666EDBBCFB0* ___0_component, const RuntimeMethod* method) 
{
	Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 V_0;
	memset((&V_0), 0, sizeof(V_0));
	Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 V_1;
	memset((&V_1), 0, sizeof(V_1));
	{
		VisualEffect_t8CF4F0E13AEBF27224BB1323CC103666EDBBCFB0* L_0 = ___0_component;
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_1 = __this->___Position;
		Transform_tB27202C6F4E36D225EE28A13E4D662BF99785DB1* L_2 = __this->___Target;
		NullCheck(L_2);
		Transform_tB27202C6F4E36D225EE28A13E4D662BF99785DB1* L_3;
		L_3 = Component_get_transform_m2919A1D81931E6932C7F06D4C2F0AB8DDA9A5371(L_2, NULL);
		VFXSpaceableBinder_ApplySpacePositionNormal_m57A07B73ED57359C970485FF3EBA907964822089(__this, L_0, L_1, L_3, (&V_0), (&V_1), NULL);
		VisualEffect_t8CF4F0E13AEBF27224BB1323CC103666EDBBCFB0* L_4 = ___0_component;
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_5 = __this->___Position;
		int32_t L_6;
		L_6 = ExposedProperty_op_Implicit_mA6E7C425842E5FA0B783360A77558C2D45A1970F(L_5, NULL);
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_7 = V_0;
		NullCheck(L_4);
		VisualEffect_SetVector3_m9DEA0241E8C6768309AEDC1319F3BD84EAB8D246(L_4, L_6, L_7, NULL);
		VisualEffect_t8CF4F0E13AEBF27224BB1323CC103666EDBBCFB0* L_8 = ___0_component;
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_9 = __this->___Normal;
		int32_t L_10;
		L_10 = ExposedProperty_op_Implicit_mA6E7C425842E5FA0B783360A77558C2D45A1970F(L_9, NULL);
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_11 = V_1;
		NullCheck(L_8);
		VisualEffect_SetVector3_m9DEA0241E8C6768309AEDC1319F3BD84EAB8D246(L_8, L_10, L_11, NULL);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* VFXPlaneBinder_ToString_mE44F0899E57BE384413B7B7955F5E6BC6E683F52 (VFXPlaneBinder_t52D64377F31144687E6984CB470F8F20DF50AB58* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteral25F810822C78319CB71CA9C122BF5E40EB3FE7B6);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteralE6971243F7EB358A7C6E0D382075D789ADC51C31);
		s_Il2CppMethodInitialized = true;
	}
	ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* G_B2_0 = NULL;
	String_t* G_B2_1 = NULL;
	ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* G_B1_0 = NULL;
	String_t* G_B1_1 = NULL;
	String_t* G_B3_0 = NULL;
	ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* G_B3_1 = NULL;
	String_t* G_B3_2 = NULL;
	{
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_0 = __this->___m_Property;
		Transform_tB27202C6F4E36D225EE28A13E4D662BF99785DB1* L_1 = __this->___Target;
		il2cpp_codegen_runtime_class_init_inline(Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C_il2cpp_TypeInfo_var);
		bool L_2;
		L_2 = Object_op_Equality_mB6120F782D83091EF56A198FCEBCF066DB4A9605(L_1, (Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C*)NULL, NULL);
		if (L_2)
		{
			G_B2_0 = L_0;
			G_B2_1 = _stringLiteral25F810822C78319CB71CA9C122BF5E40EB3FE7B6;
			goto IL_0026;
		}
		G_B1_0 = L_0;
		G_B1_1 = _stringLiteral25F810822C78319CB71CA9C122BF5E40EB3FE7B6;
	}
	{
		Transform_tB27202C6F4E36D225EE28A13E4D662BF99785DB1* L_3 = __this->___Target;
		NullCheck(L_3);
		String_t* L_4;
		L_4 = Object_get_name_mAC2F6B897CF1303BA4249B4CB55271AFACBB6392(L_3, NULL);
		G_B3_0 = L_4;
		G_B3_1 = G_B1_0;
		G_B3_2 = G_B1_1;
		goto IL_002b;
	}

IL_0026:
	{
		G_B3_0 = _stringLiteralE6971243F7EB358A7C6E0D382075D789ADC51C31;
		G_B3_1 = G_B2_0;
		G_B3_2 = G_B2_1;
	}

IL_002b:
	{
		String_t* L_5;
		L_5 = String_Format_mFB7DA489BD99F4670881FF50EC017BFB0A5C0987(G_B3_2, G_B3_1, G_B3_0, NULL);
		return L_5;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void VFXPlaneBinder__ctor_mED11CF344484C1B27051B2E34270AE1FB45765C3 (VFXPlaneBinder_t52D64377F31144687E6984CB470F8F20DF50AB58* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteral80DFB27CCC6CCF3576955208EDB98ABF86D1E5D8);
		s_Il2CppMethodInitialized = true;
	}
	{
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_0;
		L_0 = ExposedProperty_op_Implicit_m9CEB3D09454745C8E450BAB7C266D15ED28B44FB(_stringLiteral80DFB27CCC6CCF3576955208EDB98ABF86D1E5D8, NULL);
		__this->___m_Property = L_0;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___m_Property), (void*)L_0);
		VFXSpaceableBinder__ctor_m22AD01FED423E4B640D31DC8DC13AD85422E1A04(__this, NULL);
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
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* VFXPositionBinder_get_Property_m2D7DD39EC9D12CFD373412BB123D8072B96DA611 (VFXPositionBinder_t4CD7239B0C1EA2DA36502BF5D324FAF39632F8A4* __this, const RuntimeMethod* method) 
{
	{
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_0 = __this->___m_Property;
		String_t* L_1;
		L_1 = ExposedProperty_op_Explicit_m8F7C257466E30C5602C4F12AEA2588B7093424F3_inline(L_0, NULL);
		return L_1;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void VFXPositionBinder_set_Property_m3E5DF8C12D71D8DF8585AC6B60D72CFE6144FA2C (VFXPositionBinder_t4CD7239B0C1EA2DA36502BF5D324FAF39632F8A4* __this, String_t* ___0_value, const RuntimeMethod* method) 
{
	{
		String_t* L_0 = ___0_value;
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_1;
		L_1 = ExposedProperty_op_Implicit_m9CEB3D09454745C8E450BAB7C266D15ED28B44FB(L_0, NULL);
		__this->___m_Property = L_1;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___m_Property), (void*)L_1);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool VFXPositionBinder_IsValid_m35061E5E219677356561587A4D135AE338694DEA (VFXPositionBinder_t4CD7239B0C1EA2DA36502BF5D324FAF39632F8A4* __this, VisualEffect_t8CF4F0E13AEBF27224BB1323CC103666EDBBCFB0* ___0_component, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C_il2cpp_TypeInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	{
		Transform_tB27202C6F4E36D225EE28A13E4D662BF99785DB1* L_0 = __this->___Target;
		il2cpp_codegen_runtime_class_init_inline(Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C_il2cpp_TypeInfo_var);
		bool L_1;
		L_1 = Object_op_Inequality_mD0BE578448EAA61948F25C32F8DD55AB1F778602(L_0, (Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C*)NULL, NULL);
		if (!L_1)
		{
			goto IL_0020;
		}
	}
	{
		VisualEffect_t8CF4F0E13AEBF27224BB1323CC103666EDBBCFB0* L_2 = ___0_component;
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_3 = __this->___m_Property;
		int32_t L_4;
		L_4 = ExposedProperty_op_Implicit_mA6E7C425842E5FA0B783360A77558C2D45A1970F(L_3, NULL);
		NullCheck(L_2);
		bool L_5;
		L_5 = VisualEffect_HasVector3_mADAFDFDB7950BEF5AFFB2CBD59E620B79F5431C6(L_2, L_4, NULL);
		return L_5;
	}

IL_0020:
	{
		return (bool)0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void VFXPositionBinder_UpdateBinding_mD01D10DACA584A345656478A1573D7B1359C6BC5 (VFXPositionBinder_t4CD7239B0C1EA2DA36502BF5D324FAF39632F8A4* __this, VisualEffect_t8CF4F0E13AEBF27224BB1323CC103666EDBBCFB0* ___0_component, const RuntimeMethod* method) 
{
	Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		VisualEffect_t8CF4F0E13AEBF27224BB1323CC103666EDBBCFB0* L_0 = ___0_component;
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_1 = __this->___m_Property;
		Transform_tB27202C6F4E36D225EE28A13E4D662BF99785DB1* L_2 = __this->___Target;
		NullCheck(L_2);
		Transform_tB27202C6F4E36D225EE28A13E4D662BF99785DB1* L_3;
		L_3 = Component_get_transform_m2919A1D81931E6932C7F06D4C2F0AB8DDA9A5371(L_2, NULL);
		NullCheck(L_3);
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_4;
		L_4 = Transform_get_position_m69CD5FA214FDAE7BB701552943674846C220FDE1(L_3, NULL);
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_5;
		L_5 = VFXSpaceableBinder_ApplySpacePosition_mA1B55C45A80F225F7FC4562E74E306E90F95D12D(__this, L_0, L_1, L_4, NULL);
		V_0 = L_5;
		VisualEffect_t8CF4F0E13AEBF27224BB1323CC103666EDBBCFB0* L_6 = ___0_component;
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_7 = __this->___m_Property;
		int32_t L_8;
		L_8 = ExposedProperty_op_Implicit_mA6E7C425842E5FA0B783360A77558C2D45A1970F(L_7, NULL);
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_9 = V_0;
		NullCheck(L_6);
		VisualEffect_SetVector3_m9DEA0241E8C6768309AEDC1319F3BD84EAB8D246(L_6, L_8, L_9, NULL);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* VFXPositionBinder_ToString_m78DB45D14627C4B494AE4AE39C19A516F554B92F (VFXPositionBinder_t4CD7239B0C1EA2DA36502BF5D324FAF39632F8A4* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteralBCA55B34FA6BC5431A8F9B97D8C7697DFED6E45F);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteralE6971243F7EB358A7C6E0D382075D789ADC51C31);
		s_Il2CppMethodInitialized = true;
	}
	ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* G_B2_0 = NULL;
	String_t* G_B2_1 = NULL;
	ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* G_B1_0 = NULL;
	String_t* G_B1_1 = NULL;
	String_t* G_B3_0 = NULL;
	ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* G_B3_1 = NULL;
	String_t* G_B3_2 = NULL;
	{
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_0 = __this->___m_Property;
		Transform_tB27202C6F4E36D225EE28A13E4D662BF99785DB1* L_1 = __this->___Target;
		il2cpp_codegen_runtime_class_init_inline(Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C_il2cpp_TypeInfo_var);
		bool L_2;
		L_2 = Object_op_Equality_mB6120F782D83091EF56A198FCEBCF066DB4A9605(L_1, (Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C*)NULL, NULL);
		if (L_2)
		{
			G_B2_0 = L_0;
			G_B2_1 = _stringLiteralBCA55B34FA6BC5431A8F9B97D8C7697DFED6E45F;
			goto IL_0026;
		}
		G_B1_0 = L_0;
		G_B1_1 = _stringLiteralBCA55B34FA6BC5431A8F9B97D8C7697DFED6E45F;
	}
	{
		Transform_tB27202C6F4E36D225EE28A13E4D662BF99785DB1* L_3 = __this->___Target;
		NullCheck(L_3);
		String_t* L_4;
		L_4 = Object_get_name_mAC2F6B897CF1303BA4249B4CB55271AFACBB6392(L_3, NULL);
		G_B3_0 = L_4;
		G_B3_1 = G_B1_0;
		G_B3_2 = G_B1_1;
		goto IL_002b;
	}

IL_0026:
	{
		G_B3_0 = _stringLiteralE6971243F7EB358A7C6E0D382075D789ADC51C31;
		G_B3_1 = G_B2_0;
		G_B3_2 = G_B2_1;
	}

IL_002b:
	{
		String_t* L_5;
		L_5 = String_Format_mFB7DA489BD99F4670881FF50EC017BFB0A5C0987(G_B3_2, G_B3_1, G_B3_0, NULL);
		return L_5;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void VFXPositionBinder__ctor_m5CF6F416503FA2FFEFEF87579C815966DA13206A (VFXPositionBinder_t4CD7239B0C1EA2DA36502BF5D324FAF39632F8A4* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteral570A8C820BD3B01A870C767E1B234057A3ABC60F);
		s_Il2CppMethodInitialized = true;
	}
	{
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_0;
		L_0 = ExposedProperty_op_Implicit_m9CEB3D09454745C8E450BAB7C266D15ED28B44FB(_stringLiteral570A8C820BD3B01A870C767E1B234057A3ABC60F, NULL);
		__this->___m_Property = L_0;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___m_Property), (void*)L_0);
		VFXSpaceableBinder__ctor_m22AD01FED423E4B640D31DC8DC13AD85422E1A04(__this, NULL);
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
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void VFXPreviousPositionBinder_OnEnable_mE04D045DBCEF3C2A8939400C8238B1824104411D (VFXPreviousPositionBinder_t287E40B0B297056DD27888F72D017CED6BC1F524* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C_il2cpp_TypeInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	VFXPreviousPositionBinder_t287E40B0B297056DD27888F72D017CED6BC1F524* G_B2_0 = NULL;
	VFXPreviousPositionBinder_t287E40B0B297056DD27888F72D017CED6BC1F524* G_B1_0 = NULL;
	Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 G_B3_0;
	memset((&G_B3_0), 0, sizeof(G_B3_0));
	VFXPreviousPositionBinder_t287E40B0B297056DD27888F72D017CED6BC1F524* G_B3_1 = NULL;
	{
		VFXBinderBase_OnEnable_mA026DDFCBD31D8800C70F6A291A8DB6387F2041E(__this, NULL);
		Transform_tB27202C6F4E36D225EE28A13E4D662BF99785DB1* L_0 = __this->___Target;
		il2cpp_codegen_runtime_class_init_inline(Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C_il2cpp_TypeInfo_var);
		bool L_1;
		L_1 = Object_op_Inequality_mD0BE578448EAA61948F25C32F8DD55AB1F778602(L_0, (Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C*)NULL, NULL);
		if (L_1)
		{
			G_B2_0 = __this;
			goto IL_001c;
		}
		G_B1_0 = __this;
	}
	{
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_2;
		L_2 = Vector3_get_zero_m0C1249C3F25B1C70EAD3CC8B31259975A457AE39_inline(NULL);
		G_B3_0 = L_2;
		G_B3_1 = G_B1_0;
		goto IL_0027;
	}

IL_001c:
	{
		Transform_tB27202C6F4E36D225EE28A13E4D662BF99785DB1* L_3 = __this->___Target;
		NullCheck(L_3);
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_4;
		L_4 = Transform_get_position_m69CD5FA214FDAE7BB701552943674846C220FDE1(L_3, NULL);
		G_B3_0 = L_4;
		G_B3_1 = G_B2_0;
	}

IL_0027:
	{
		NullCheck(G_B3_1);
		G_B3_1->___oldPosition = G_B3_0;
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool VFXPreviousPositionBinder_IsValid_mB7EB49FEBF47D5DD3137CC3FB077CCAE1DA62C76 (VFXPreviousPositionBinder_t287E40B0B297056DD27888F72D017CED6BC1F524* __this, VisualEffect_t8CF4F0E13AEBF27224BB1323CC103666EDBBCFB0* ___0_component, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C_il2cpp_TypeInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	{
		Transform_tB27202C6F4E36D225EE28A13E4D662BF99785DB1* L_0 = __this->___Target;
		il2cpp_codegen_runtime_class_init_inline(Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C_il2cpp_TypeInfo_var);
		bool L_1;
		L_1 = Object_op_Inequality_mD0BE578448EAA61948F25C32F8DD55AB1F778602(L_0, (Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C*)NULL, NULL);
		if (!L_1)
		{
			goto IL_0020;
		}
	}
	{
		VisualEffect_t8CF4F0E13AEBF27224BB1323CC103666EDBBCFB0* L_2 = ___0_component;
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_3 = __this->___m_Property;
		int32_t L_4;
		L_4 = ExposedProperty_op_Implicit_mA6E7C425842E5FA0B783360A77558C2D45A1970F(L_3, NULL);
		NullCheck(L_2);
		bool L_5;
		L_5 = VisualEffect_HasVector3_mADAFDFDB7950BEF5AFFB2CBD59E620B79F5431C6(L_2, L_4, NULL);
		return L_5;
	}

IL_0020:
	{
		return (bool)0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void VFXPreviousPositionBinder_UpdateBinding_mC5F3579B2C52ECBC389AB59BD446B6FE2119B7A6 (VFXPreviousPositionBinder_t287E40B0B297056DD27888F72D017CED6BC1F524* __this, VisualEffect_t8CF4F0E13AEBF27224BB1323CC103666EDBBCFB0* ___0_component, const RuntimeMethod* method) 
{
	Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		VisualEffect_t8CF4F0E13AEBF27224BB1323CC103666EDBBCFB0* L_0 = ___0_component;
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_1 = __this->___m_Property;
		int32_t L_2;
		L_2 = ExposedProperty_op_Implicit_mA6E7C425842E5FA0B783360A77558C2D45A1970F(L_1, NULL);
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_3 = __this->___oldPosition;
		NullCheck(L_0);
		VisualEffect_SetVector3_m9DEA0241E8C6768309AEDC1319F3BD84EAB8D246(L_0, L_2, L_3, NULL);
		VisualEffect_t8CF4F0E13AEBF27224BB1323CC103666EDBBCFB0* L_4 = ___0_component;
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_5 = __this->___m_Property;
		Transform_tB27202C6F4E36D225EE28A13E4D662BF99785DB1* L_6 = __this->___Target;
		NullCheck(L_6);
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_7;
		L_7 = Transform_get_position_m69CD5FA214FDAE7BB701552943674846C220FDE1(L_6, NULL);
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_8;
		L_8 = VFXSpaceableBinder_ApplySpacePosition_mA1B55C45A80F225F7FC4562E74E306E90F95D12D(__this, L_4, L_5, L_7, NULL);
		V_0 = L_8;
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_9 = V_0;
		__this->___oldPosition = L_9;
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* VFXPreviousPositionBinder_ToString_m9CE4B7425D39CC7001D4345EB23DAF871DB4E81C (VFXPreviousPositionBinder_t287E40B0B297056DD27888F72D017CED6BC1F524* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteral6978D594BBF5E4D716F6B6F9237F05A81DE62CB4);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteralE6971243F7EB358A7C6E0D382075D789ADC51C31);
		s_Il2CppMethodInitialized = true;
	}
	ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* G_B2_0 = NULL;
	String_t* G_B2_1 = NULL;
	ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* G_B1_0 = NULL;
	String_t* G_B1_1 = NULL;
	String_t* G_B3_0 = NULL;
	ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* G_B3_1 = NULL;
	String_t* G_B3_2 = NULL;
	{
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_0 = __this->___m_Property;
		Transform_tB27202C6F4E36D225EE28A13E4D662BF99785DB1* L_1 = __this->___Target;
		il2cpp_codegen_runtime_class_init_inline(Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C_il2cpp_TypeInfo_var);
		bool L_2;
		L_2 = Object_op_Equality_mB6120F782D83091EF56A198FCEBCF066DB4A9605(L_1, (Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C*)NULL, NULL);
		if (L_2)
		{
			G_B2_0 = L_0;
			G_B2_1 = _stringLiteral6978D594BBF5E4D716F6B6F9237F05A81DE62CB4;
			goto IL_0026;
		}
		G_B1_0 = L_0;
		G_B1_1 = _stringLiteral6978D594BBF5E4D716F6B6F9237F05A81DE62CB4;
	}
	{
		Transform_tB27202C6F4E36D225EE28A13E4D662BF99785DB1* L_3 = __this->___Target;
		NullCheck(L_3);
		String_t* L_4;
		L_4 = Object_get_name_mAC2F6B897CF1303BA4249B4CB55271AFACBB6392(L_3, NULL);
		G_B3_0 = L_4;
		G_B3_1 = G_B1_0;
		G_B3_2 = G_B1_1;
		goto IL_002b;
	}

IL_0026:
	{
		G_B3_0 = _stringLiteralE6971243F7EB358A7C6E0D382075D789ADC51C31;
		G_B3_1 = G_B2_0;
		G_B3_2 = G_B2_1;
	}

IL_002b:
	{
		String_t* L_5;
		L_5 = String_Format_mFB7DA489BD99F4670881FF50EC017BFB0A5C0987(G_B3_2, G_B3_1, G_B3_0, NULL);
		return L_5;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void VFXPreviousPositionBinder__ctor_m08FD8E72C2B94BACB7B3C6765A4EB267889DA429 (VFXPreviousPositionBinder_t287E40B0B297056DD27888F72D017CED6BC1F524* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteral25CE67F682345294AA5AF9F9B11847064BADF8E7);
		s_Il2CppMethodInitialized = true;
	}
	{
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_0;
		L_0 = ExposedProperty_op_Implicit_m9CEB3D09454745C8E450BAB7C266D15ED28B44FB(_stringLiteral25CE67F682345294AA5AF9F9B11847064BADF8E7, NULL);
		__this->___m_Property = L_0;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___m_Property), (void*)L_0);
		VFXSpaceableBinder__ctor_m22AD01FED423E4B640D31DC8DC13AD85422E1A04(__this, NULL);
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
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* VFXRaycastBinder_get_TargetPosition_mB1DA401AC6AE1AAE043647B165928BA37624F56D (VFXRaycastBinder_tD681421294476C02004DF02A34F958796887ED17* __this, const RuntimeMethod* method) 
{
	{
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_0 = __this->___m_TargetPosition;
		String_t* L_1;
		L_1 = ExposedProperty_op_Explicit_m8F7C257466E30C5602C4F12AEA2588B7093424F3_inline(L_0, NULL);
		return L_1;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void VFXRaycastBinder_set_TargetPosition_mA7F2AB642781472A6E17C0C9EF1A0B9CE5623C55 (VFXRaycastBinder_tD681421294476C02004DF02A34F958796887ED17* __this, String_t* ___0_value, const RuntimeMethod* method) 
{
	{
		String_t* L_0 = ___0_value;
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_1;
		L_1 = ExposedProperty_op_Implicit_m9CEB3D09454745C8E450BAB7C266D15ED28B44FB(L_0, NULL);
		__this->___m_TargetPosition = L_1;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___m_TargetPosition), (void*)L_1);
		VFXRaycastBinder_UpdateSubProperties_m984FC69B108C955BA03E05B08DEC583AAFFDE01D(__this, NULL);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* VFXRaycastBinder_get_TargetNormal_mF80F5F53B038FE6044A3B3BC2B05A3C7A0D26A09 (VFXRaycastBinder_tD681421294476C02004DF02A34F958796887ED17* __this, const RuntimeMethod* method) 
{
	{
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_0 = __this->___m_TargetNormal;
		String_t* L_1;
		L_1 = ExposedProperty_op_Explicit_m8F7C257466E30C5602C4F12AEA2588B7093424F3_inline(L_0, NULL);
		return L_1;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void VFXRaycastBinder_set_TargetNormal_mC914237651B9C4FACDBAC0013BE1F06DE7AD3A58 (VFXRaycastBinder_tD681421294476C02004DF02A34F958796887ED17* __this, String_t* ___0_value, const RuntimeMethod* method) 
{
	{
		String_t* L_0 = ___0_value;
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_1;
		L_1 = ExposedProperty_op_Implicit_m9CEB3D09454745C8E450BAB7C266D15ED28B44FB(L_0, NULL);
		__this->___m_TargetNormal = L_1;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___m_TargetNormal), (void*)L_1);
		VFXRaycastBinder_UpdateSubProperties_m984FC69B108C955BA03E05B08DEC583AAFFDE01D(__this, NULL);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* VFXRaycastBinder_get_TargetHit_mCCE5712B9F489C7E060B1E7753D3F441852E8F1F (VFXRaycastBinder_tD681421294476C02004DF02A34F958796887ED17* __this, const RuntimeMethod* method) 
{
	{
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_0 = __this->___m_TargetHit;
		String_t* L_1;
		L_1 = ExposedProperty_op_Explicit_m8F7C257466E30C5602C4F12AEA2588B7093424F3_inline(L_0, NULL);
		return L_1;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void VFXRaycastBinder_set_TargetHit_m14E7AE924CD526CF36C41DB84BF94E07706CE5CE (VFXRaycastBinder_tD681421294476C02004DF02A34F958796887ED17* __this, String_t* ___0_value, const RuntimeMethod* method) 
{
	{
		String_t* L_0 = ___0_value;
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_1;
		L_1 = ExposedProperty_op_Implicit_m9CEB3D09454745C8E450BAB7C266D15ED28B44FB(L_0, NULL);
		__this->___m_TargetHit = L_1;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___m_TargetHit), (void*)L_1);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void VFXRaycastBinder_OnEnable_m3970D4A7E5262CE38A895CAE3CBB22E9FE801678 (VFXRaycastBinder_tD681421294476C02004DF02A34F958796887ED17* __this, const RuntimeMethod* method) 
{
	{
		VFXBinderBase_OnEnable_mA026DDFCBD31D8800C70F6A291A8DB6387F2041E(__this, NULL);
		VFXRaycastBinder_UpdateSubProperties_m984FC69B108C955BA03E05B08DEC583AAFFDE01D(__this, NULL);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void VFXRaycastBinder_OnValidate_m946F4F1F8F9ED30E1657D07CD472330C83C8D0D2 (VFXRaycastBinder_tD681421294476C02004DF02A34F958796887ED17* __this, const RuntimeMethod* method) 
{
	{
		VFXRaycastBinder_UpdateSubProperties_m984FC69B108C955BA03E05B08DEC583AAFFDE01D(__this, NULL);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void VFXRaycastBinder_UpdateSubProperties_m984FC69B108C955BA03E05B08DEC583AAFFDE01D (VFXRaycastBinder_tD681421294476C02004DF02A34F958796887ED17* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteralAAA301CFC06E306888A9185B3D14CBEEA6C6A2C4);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteralCA4FE6DE4D088688B7375BECF5966DF1682F104D);
		s_Il2CppMethodInitialized = true;
	}
	{
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_0 = __this->___m_TargetPosition;
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_1;
		L_1 = ExposedProperty_op_Implicit_m9CEB3D09454745C8E450BAB7C266D15ED28B44FB(_stringLiteralCA4FE6DE4D088688B7375BECF5966DF1682F104D, NULL);
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_2;
		L_2 = ExposedProperty_op_Addition_m2FBBFB0417DEA6D43A730115FA3B9F39A3892C93(L_0, L_1, NULL);
		__this->___m_TargetPosition_position = L_2;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___m_TargetPosition_position), (void*)L_2);
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_3 = __this->___m_TargetNormal;
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_4;
		L_4 = ExposedProperty_op_Implicit_m9CEB3D09454745C8E450BAB7C266D15ED28B44FB(_stringLiteralAAA301CFC06E306888A9185B3D14CBEEA6C6A2C4, NULL);
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_5;
		L_5 = ExposedProperty_op_Addition_m2FBBFB0417DEA6D43A730115FA3B9F39A3892C93(L_3, L_4, NULL);
		__this->___m_TargetNormal_direction = L_5;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___m_TargetNormal_direction), (void*)L_5);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool VFXRaycastBinder_IsValid_m90C04243E0957E1BC9EF550A17844A6DA60E6960 (VFXRaycastBinder_tD681421294476C02004DF02A34F958796887ED17* __this, VisualEffect_t8CF4F0E13AEBF27224BB1323CC103666EDBBCFB0* ___0_component, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C_il2cpp_TypeInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	{
		VisualEffect_t8CF4F0E13AEBF27224BB1323CC103666EDBBCFB0* L_0 = ___0_component;
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_1 = __this->___m_TargetPosition_position;
		int32_t L_2;
		L_2 = ExposedProperty_op_Implicit_mA6E7C425842E5FA0B783360A77558C2D45A1970F(L_1, NULL);
		NullCheck(L_0);
		bool L_3;
		L_3 = VisualEffect_HasVector3_mADAFDFDB7950BEF5AFFB2CBD59E620B79F5431C6(L_0, L_2, NULL);
		if (!L_3)
		{
			goto IL_0046;
		}
	}
	{
		VisualEffect_t8CF4F0E13AEBF27224BB1323CC103666EDBBCFB0* L_4 = ___0_component;
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_5 = __this->___m_TargetNormal_direction;
		int32_t L_6;
		L_6 = ExposedProperty_op_Implicit_mA6E7C425842E5FA0B783360A77558C2D45A1970F(L_5, NULL);
		NullCheck(L_4);
		bool L_7;
		L_7 = VisualEffect_HasVector3_mADAFDFDB7950BEF5AFFB2CBD59E620B79F5431C6(L_4, L_6, NULL);
		if (!L_7)
		{
			goto IL_0046;
		}
	}
	{
		VisualEffect_t8CF4F0E13AEBF27224BB1323CC103666EDBBCFB0* L_8 = ___0_component;
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_9 = __this->___m_TargetHit;
		int32_t L_10;
		L_10 = ExposedProperty_op_Implicit_mA6E7C425842E5FA0B783360A77558C2D45A1970F(L_9, NULL);
		NullCheck(L_8);
		bool L_11;
		L_11 = VisualEffect_HasBool_m880EB204D4723EB3C06B2E34A52486E678A752AA(L_8, L_10, NULL);
		if (!L_11)
		{
			goto IL_0046;
		}
	}
	{
		GameObject_t76FEDD663AB33C991A9C9A23129337651094216F* L_12 = __this->___RaycastSource;
		il2cpp_codegen_runtime_class_init_inline(Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C_il2cpp_TypeInfo_var);
		bool L_13;
		L_13 = Object_op_Inequality_mD0BE578448EAA61948F25C32F8DD55AB1F778602(L_12, (Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C*)NULL, NULL);
		return L_13;
	}

IL_0046:
	{
		return (bool)0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void VFXRaycastBinder_UpdateBinding_m16F89A49C9B46B21A4891CF80A6A61268C5E0188 (VFXRaycastBinder_tD681421294476C02004DF02A34F958796887ED17* __this, VisualEffect_t8CF4F0E13AEBF27224BB1323CC103666EDBBCFB0* ___0_component, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Physics_t1244C2983AEAFA149425AFFC3DF53BC91C18ED56_il2cpp_TypeInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 V_0;
	memset((&V_0), 0, sizeof(V_0));
	bool V_1 = false;
	Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 G_B3_0;
	memset((&G_B3_0), 0, sizeof(G_B3_0));
	{
		int32_t L_0 = __this->___RaycastDirectionSpace;
		if (!L_0)
		{
			goto IL_0010;
		}
	}
	{
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_1 = __this->___RaycastDirection;
		G_B3_0 = L_1;
		goto IL_0026;
	}

IL_0010:
	{
		GameObject_t76FEDD663AB33C991A9C9A23129337651094216F* L_2 = __this->___RaycastSource;
		NullCheck(L_2);
		Transform_tB27202C6F4E36D225EE28A13E4D662BF99785DB1* L_3;
		L_3 = GameObject_get_transform_m0BC10ADFA1632166AE5544BDF9038A2650C2AE56(L_2, NULL);
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_4 = __this->___RaycastDirection;
		NullCheck(L_3);
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_5;
		L_5 = Transform_TransformDirection_m9BE1261DF2D48B7A4A27D31EE24D2D97F89E7757(L_3, L_4, NULL);
		G_B3_0 = L_5;
	}

IL_0026:
	{
		V_0 = G_B3_0;
		GameObject_t76FEDD663AB33C991A9C9A23129337651094216F* L_6 = __this->___RaycastSource;
		NullCheck(L_6);
		Transform_tB27202C6F4E36D225EE28A13E4D662BF99785DB1* L_7;
		L_7 = GameObject_get_transform_m0BC10ADFA1632166AE5544BDF9038A2650C2AE56(L_6, NULL);
		NullCheck(L_7);
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_8;
		L_8 = Transform_get_position_m69CD5FA214FDAE7BB701552943674846C220FDE1(L_7, NULL);
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_9 = V_0;
		Ray_t2B1742D7958DC05BDC3EFC7461D3593E1430DC00 L_10;
		memset((&L_10), 0, sizeof(L_10));
		Ray__ctor_mE298992FD10A3894C38373198385F345C58BD64C_inline((&L_10), L_8, L_9, NULL);
		RaycastHit_t6F30BD0B38B56401CA833A1B87BD74F2ACD2F2B5* L_11 = (RaycastHit_t6F30BD0B38B56401CA833A1B87BD74F2ACD2F2B5*)(&__this->___m_HitInfo);
		float L_12 = __this->___MaxDistance;
		LayerMask_t97CB6BDADEDC3D6423C7BCFEA7F86DA2EC6241DB L_13 = __this->___Layers;
		int32_t L_14;
		L_14 = LayerMask_op_Implicit_m7F5A5B9D079281AC445ED39DEE1FCFA9D795810D(L_13, NULL);
		il2cpp_codegen_runtime_class_init_inline(Physics_t1244C2983AEAFA149425AFFC3DF53BC91C18ED56_il2cpp_TypeInfo_var);
		bool L_15;
		L_15 = Physics_Raycast_m34AC1210E893A9EF969BD2C7104B10BE5B580025(L_10, L_11, L_12, L_14, NULL);
		V_1 = L_15;
		VisualEffect_t8CF4F0E13AEBF27224BB1323CC103666EDBBCFB0* L_16 = ___0_component;
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_17 = __this->___m_TargetPosition_position;
		int32_t L_18;
		L_18 = ExposedProperty_op_Implicit_mA6E7C425842E5FA0B783360A77558C2D45A1970F(L_17, NULL);
		RaycastHit_t6F30BD0B38B56401CA833A1B87BD74F2ACD2F2B5* L_19 = (RaycastHit_t6F30BD0B38B56401CA833A1B87BD74F2ACD2F2B5*)(&__this->___m_HitInfo);
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_20;
		L_20 = RaycastHit_get_point_m02B764612562AFE0F998CC7CFB2EEDE41BA47F39(L_19, NULL);
		NullCheck(L_16);
		VisualEffect_SetVector3_m9DEA0241E8C6768309AEDC1319F3BD84EAB8D246(L_16, L_18, L_20, NULL);
		VisualEffect_t8CF4F0E13AEBF27224BB1323CC103666EDBBCFB0* L_21 = ___0_component;
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_22 = __this->___m_TargetNormal_direction;
		int32_t L_23;
		L_23 = ExposedProperty_op_Implicit_mA6E7C425842E5FA0B783360A77558C2D45A1970F(L_22, NULL);
		RaycastHit_t6F30BD0B38B56401CA833A1B87BD74F2ACD2F2B5* L_24 = (RaycastHit_t6F30BD0B38B56401CA833A1B87BD74F2ACD2F2B5*)(&__this->___m_HitInfo);
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_25;
		L_25 = RaycastHit_get_normal_mD8741B70D2039C5CAFC4368D4CE59D89562040B5(L_24, NULL);
		NullCheck(L_21);
		VisualEffect_SetVector3_m9DEA0241E8C6768309AEDC1319F3BD84EAB8D246(L_21, L_23, L_25, NULL);
		VisualEffect_t8CF4F0E13AEBF27224BB1323CC103666EDBBCFB0* L_26 = ___0_component;
		String_t* L_27;
		L_27 = VFXRaycastBinder_get_TargetHit_mCCE5712B9F489C7E060B1E7753D3F441852E8F1F(__this, NULL);
		bool L_28 = V_1;
		NullCheck(L_26);
		VisualEffect_SetBool_m2AE92C97A9290E260655D557C4E68B2947E72964(L_26, L_27, L_28, NULL);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* VFXRaycastBinder_ToString_mA78937DE31C9774C3FC1ED4EA455DAC05F90937A (VFXRaycastBinder_tD681421294476C02004DF02A34F958796887ED17* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Array_Empty_TisRuntimeObject_mFB8A63D602BB6974D31E20300D9EB89C6FE7C278_RuntimeMethod_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Space_tC1A1B173D794CCED4F69F7B78C6C9D50C69C7FBE_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteral0CA784FE421CD456ED98BEAB512E327C4C927A88);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteral5BEFD8CC60A79699B5BB00E37BAC5B62D371E174);
		s_Il2CppMethodInitialized = true;
	}
	String_t* G_B2_0 = NULL;
	String_t* G_B1_0 = NULL;
	String_t* G_B3_0 = NULL;
	String_t* G_B3_1 = NULL;
	{
		GameObject_t76FEDD663AB33C991A9C9A23129337651094216F* L_0 = __this->___RaycastSource;
		il2cpp_codegen_runtime_class_init_inline(Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C_il2cpp_TypeInfo_var);
		bool L_1;
		L_1 = Object_op_Equality_mB6120F782D83091EF56A198FCEBCF066DB4A9605(L_0, (Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C*)NULL, NULL);
		if (L_1)
		{
			G_B2_0 = _stringLiteral0CA784FE421CD456ED98BEAB512E327C4C927A88;
			goto IL_0020;
		}
		G_B1_0 = _stringLiteral0CA784FE421CD456ED98BEAB512E327C4C927A88;
	}
	{
		GameObject_t76FEDD663AB33C991A9C9A23129337651094216F* L_2 = __this->___RaycastSource;
		NullCheck(L_2);
		String_t* L_3;
		L_3 = Object_get_name_mAC2F6B897CF1303BA4249B4CB55271AFACBB6392(L_2, NULL);
		G_B3_0 = L_3;
		G_B3_1 = G_B1_0;
		goto IL_0025;
	}

IL_0020:
	{
		G_B3_0 = _stringLiteral5BEFD8CC60A79699B5BB00E37BAC5B62D371E174;
		G_B3_1 = G_B2_0;
	}

IL_0025:
	{
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_4 = __this->___RaycastDirection;
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_5 = L_4;
		RuntimeObject* L_6 = Box(Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2_il2cpp_TypeInfo_var, &L_5);
		int32_t L_7 = __this->___RaycastDirectionSpace;
		int32_t L_8 = L_7;
		RuntimeObject* L_9 = Box(Space_tC1A1B173D794CCED4F69F7B78C6C9D50C69C7FBE_il2cpp_TypeInfo_var, &L_8);
		String_t* L_10;
		L_10 = String_Format_mA0534D6E2AE4D67A6BD8D45B3321323930EB930C(G_B3_1, G_B3_0, L_6, L_9, NULL);
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_11;
		L_11 = Array_Empty_TisRuntimeObject_mFB8A63D602BB6974D31E20300D9EB89C6FE7C278_inline(Array_Empty_TisRuntimeObject_mFB8A63D602BB6974D31E20300D9EB89C6FE7C278_RuntimeMethod_var);
		String_t* L_12;
		L_12 = String_Format_m918500C1EFB475181349A79989BB79BB36102894(L_10, L_11, NULL);
		return L_12;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void VFXRaycastBinder__ctor_mDD2986E1D00C5C668849ED855384DA39638DDFB0 (VFXRaycastBinder_tD681421294476C02004DF02A34F958796887ED17* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteral666AF83F7D0247EED6BA0E39B022A08AF49F95C6);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteral6CD7D939B718E76DDCB166380F92116EA604BA0E);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteral76784E8E2FFDA985828158A43B730DE1F4684442);
		s_Il2CppMethodInitialized = true;
	}
	{
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_0;
		L_0 = ExposedProperty_op_Implicit_m9CEB3D09454745C8E450BAB7C266D15ED28B44FB(_stringLiteral6CD7D939B718E76DDCB166380F92116EA604BA0E, NULL);
		__this->___m_TargetPosition = L_0;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___m_TargetPosition), (void*)L_0);
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_1;
		L_1 = ExposedProperty_op_Implicit_m9CEB3D09454745C8E450BAB7C266D15ED28B44FB(_stringLiteral76784E8E2FFDA985828158A43B730DE1F4684442, NULL);
		__this->___m_TargetNormal = L_1;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___m_TargetNormal), (void*)L_1);
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_2;
		L_2 = ExposedProperty_op_Implicit_m9CEB3D09454745C8E450BAB7C266D15ED28B44FB(_stringLiteral666AF83F7D0247EED6BA0E39B022A08AF49F95C6, NULL);
		__this->___m_TargetHit = L_2;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___m_TargetHit), (void*)L_2);
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_3;
		L_3 = Vector3_get_forward_mAA55A7034304DF8B2152EAD49AE779FC4CA2EB4A_inline(NULL);
		__this->___RaycastDirection = L_3;
		LayerMask_t97CB6BDADEDC3D6423C7BCFEA7F86DA2EC6241DB L_4;
		L_4 = LayerMask_op_Implicit_m01C8996A2CB2085328B9C33539C43139660D8222((-1), NULL);
		__this->___Layers = L_4;
		__this->___MaxDistance = (100.0f);
		VFXBinderBase__ctor_m4C0835B8FFF0968BC5378A1D579ABD0168ECF7D3(__this, NULL);
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
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t VFXSpaceableBinder_GetTargetSpace_m0137508DC0E097C46C7EAA1ECC89398947D10D58 (VFXSpaceableBinder_t0C24970B59F1F8E2DA67992F0E90DF951BF91706* __this, VisualEffect_t8CF4F0E13AEBF27224BB1323CC103666EDBBCFB0* ___0_component, ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* ___1_targetProperty, const RuntimeMethod* method) 
{
	int32_t V_0 = 0;
	int32_t V_1 = 0;
	{
		V_0 = (-1);
		int32_t L_0 = __this->___Space;
		V_1 = L_0;
		int32_t L_1 = V_1;
		switch (L_1)
		{
			case 0:
			{
				goto IL_001d;
			}
			case 1:
			{
				goto IL_0031;
			}
			case 2:
			{
				goto IL_0035;
			}
		}
	}
	{
		goto IL_0037;
	}

IL_001d:
	{
		VisualEffect_t8CF4F0E13AEBF27224BB1323CC103666EDBBCFB0* L_2 = ___0_component;
		NullCheck(L_2);
		VisualEffectAsset_t0CD57FD9980CD68033BD96DA356697F92A69E688* L_3;
		L_3 = VisualEffect_get_visualEffectAsset_m301FCE98B138CAB2E16B46CE365538ECA8AF5F00(L_2, NULL);
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_4 = ___1_targetProperty;
		int32_t L_5;
		L_5 = ExposedProperty_op_Implicit_mA6E7C425842E5FA0B783360A77558C2D45A1970F(L_4, NULL);
		NullCheck(L_3);
		int32_t L_6;
		L_6 = VisualEffectAsset_GetExposedSpace_m74640B9B45BE16A265F05649FC731EB9E3C4BE5B(L_3, L_5, NULL);
		V_0 = L_6;
		goto IL_0037;
	}

IL_0031:
	{
		V_0 = 1;
		goto IL_0037;
	}

IL_0035:
	{
		V_0 = 0;
	}

IL_0037:
	{
		int32_t L_7 = V_0;
		return L_7;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void VFXSpaceableBinder_ApplySpacePositionNormal_m57A07B73ED57359C970485FF3EBA907964822089 (VFXSpaceableBinder_t0C24970B59F1F8E2DA67992F0E90DF951BF91706* __this, VisualEffect_t8CF4F0E13AEBF27224BB1323CC103666EDBBCFB0* ___0_component, ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* ___1_targetProperty, Transform_tB27202C6F4E36D225EE28A13E4D662BF99785DB1* ___2_sourceTransform, Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2* ___3_position, Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2* ___4_normal, const RuntimeMethod* method) 
{
	Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		VisualEffect_t8CF4F0E13AEBF27224BB1323CC103666EDBBCFB0* L_0 = ___0_component;
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_1 = ___1_targetProperty;
		int32_t L_2;
		L_2 = VFXSpaceableBinder_GetTargetSpace_m0137508DC0E097C46C7EAA1ECC89398947D10D58(__this, L_0, L_1, NULL);
		if (L_2)
		{
			goto IL_0043;
		}
	}
	{
		VisualEffect_t8CF4F0E13AEBF27224BB1323CC103666EDBBCFB0* L_3 = ___0_component;
		NullCheck(L_3);
		Transform_tB27202C6F4E36D225EE28A13E4D662BF99785DB1* L_4;
		L_4 = Component_get_transform_m2919A1D81931E6932C7F06D4C2F0AB8DDA9A5371(L_3, NULL);
		NullCheck(L_4);
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_5;
		L_5 = Transform_get_worldToLocalMatrix_mB633C122A01BCE8E51B10B8B8CB95F580750B3F1(L_4, NULL);
		Transform_tB27202C6F4E36D225EE28A13E4D662BF99785DB1* L_6 = ___2_sourceTransform;
		NullCheck(L_6);
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_7;
		L_7 = Transform_get_localToWorldMatrix_m5D35188766856338DD21DE756F42277C21719E6D(L_6, NULL);
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_8;
		L_8 = Matrix4x4_op_Multiply_m75E91775655DCA8DFC8EDE0AB787285BB3935162_inline(L_5, L_7, NULL);
		V_0 = L_8;
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2* L_9 = ___3_position;
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_10;
		L_10 = Matrix4x4_GetPosition_m9C2F34474967E47C83886A2EC335DFE37B563E80_inline((&V_0), NULL);
		*(Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2*)L_9 = L_10;
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2* L_11 = ___4_normal;
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_12;
		L_12 = Vector3_get_up_m128AF3FDC820BF59D5DE86D973E7DE3F20C3AEBA_inline(NULL);
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_13;
		L_13 = Matrix4x4_MultiplyVector_mFD12F86A473E90BBB0002149ABA3917B2A518937_inline((&V_0), L_12, NULL);
		*(Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2*)L_11 = L_13;
		return;
	}

IL_0043:
	{
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2* L_14 = ___3_position;
		Transform_tB27202C6F4E36D225EE28A13E4D662BF99785DB1* L_15 = ___2_sourceTransform;
		NullCheck(L_15);
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_16;
		L_16 = Transform_get_position_m69CD5FA214FDAE7BB701552943674846C220FDE1(L_15, NULL);
		*(Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2*)L_14 = L_16;
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2* L_17 = ___4_normal;
		Transform_tB27202C6F4E36D225EE28A13E4D662BF99785DB1* L_18 = ___2_sourceTransform;
		NullCheck(L_18);
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_19;
		L_19 = Transform_get_up_mE47A9D9D96422224DD0539AA5524DA5440145BB2(L_18, NULL);
		*(Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2*)L_17 = L_19;
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void VFXSpaceableBinder_ApplySpaceTS_m3C7E145FA7A6D8B485B8992B7992FA984D76977C (VFXSpaceableBinder_t0C24970B59F1F8E2DA67992F0E90DF951BF91706* __this, VisualEffect_t8CF4F0E13AEBF27224BB1323CC103666EDBBCFB0* ___0_component, ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* ___1_targetProperty, Transform_tB27202C6F4E36D225EE28A13E4D662BF99785DB1* ___2_sourceTransform, Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2* ___3_position, Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2* ___4_scale, const RuntimeMethod* method) 
{
	Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		VisualEffect_t8CF4F0E13AEBF27224BB1323CC103666EDBBCFB0* L_0 = ___0_component;
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_1 = ___1_targetProperty;
		int32_t L_2;
		L_2 = VFXSpaceableBinder_GetTargetSpace_m0137508DC0E097C46C7EAA1ECC89398947D10D58(__this, L_0, L_1, NULL);
		if (L_2)
		{
			goto IL_003e;
		}
	}
	{
		VisualEffect_t8CF4F0E13AEBF27224BB1323CC103666EDBBCFB0* L_3 = ___0_component;
		NullCheck(L_3);
		Transform_tB27202C6F4E36D225EE28A13E4D662BF99785DB1* L_4;
		L_4 = Component_get_transform_m2919A1D81931E6932C7F06D4C2F0AB8DDA9A5371(L_3, NULL);
		NullCheck(L_4);
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_5;
		L_5 = Transform_get_worldToLocalMatrix_mB633C122A01BCE8E51B10B8B8CB95F580750B3F1(L_4, NULL);
		Transform_tB27202C6F4E36D225EE28A13E4D662BF99785DB1* L_6 = ___2_sourceTransform;
		NullCheck(L_6);
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_7;
		L_7 = Transform_get_localToWorldMatrix_m5D35188766856338DD21DE756F42277C21719E6D(L_6, NULL);
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_8;
		L_8 = Matrix4x4_op_Multiply_m75E91775655DCA8DFC8EDE0AB787285BB3935162_inline(L_5, L_7, NULL);
		V_0 = L_8;
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2* L_9 = ___3_position;
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_10;
		L_10 = Matrix4x4_GetPosition_m9C2F34474967E47C83886A2EC335DFE37B563E80_inline((&V_0), NULL);
		*(Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2*)L_9 = L_10;
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2* L_11 = ___4_scale;
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_12;
		L_12 = Matrix4x4_get_lossyScale_mFB3D4CF6408D710D607CA1D2AF94B2E4E0B57EB7_inline((&V_0), NULL);
		*(Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2*)L_11 = L_12;
		return;
	}

IL_003e:
	{
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2* L_13 = ___3_position;
		Transform_tB27202C6F4E36D225EE28A13E4D662BF99785DB1* L_14 = ___2_sourceTransform;
		NullCheck(L_14);
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_15;
		L_15 = Transform_get_position_m69CD5FA214FDAE7BB701552943674846C220FDE1(L_14, NULL);
		*(Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2*)L_13 = L_15;
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2* L_16 = ___4_scale;
		Transform_tB27202C6F4E36D225EE28A13E4D662BF99785DB1* L_17 = ___2_sourceTransform;
		NullCheck(L_17);
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_18;
		L_18 = Transform_get_lossyScale_mFF740DA4BE1489C6882CD2F3A37B7321176E5D07(L_17, NULL);
		*(Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2*)L_16 = L_18;
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void VFXSpaceableBinder_ApplySpaceTRS_m385C4C16AB3E5A84044D1406394ABC9B1A9BB748 (VFXSpaceableBinder_t0C24970B59F1F8E2DA67992F0E90DF951BF91706* __this, VisualEffect_t8CF4F0E13AEBF27224BB1323CC103666EDBBCFB0* ___0_component, ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* ___1_targetProperty, Transform_tB27202C6F4E36D225EE28A13E4D662BF99785DB1* ___2_sourceTransform, Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2* ___3_position, Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2* ___4_eulerAngles, Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2* ___5_scale, const RuntimeMethod* method) 
{
	Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 V_0;
	memset((&V_0), 0, sizeof(V_0));
	Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974 V_1;
	memset((&V_1), 0, sizeof(V_1));
	{
		VisualEffect_t8CF4F0E13AEBF27224BB1323CC103666EDBBCFB0* L_0 = ___0_component;
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_1 = ___1_targetProperty;
		int32_t L_2;
		L_2 = VFXSpaceableBinder_GetTargetSpace_m0137508DC0E097C46C7EAA1ECC89398947D10D58(__this, L_0, L_1, NULL);
		if (L_2)
		{
			goto IL_0054;
		}
	}
	{
		VisualEffect_t8CF4F0E13AEBF27224BB1323CC103666EDBBCFB0* L_3 = ___0_component;
		NullCheck(L_3);
		Transform_tB27202C6F4E36D225EE28A13E4D662BF99785DB1* L_4;
		L_4 = Component_get_transform_m2919A1D81931E6932C7F06D4C2F0AB8DDA9A5371(L_3, NULL);
		NullCheck(L_4);
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_5;
		L_5 = Transform_get_worldToLocalMatrix_mB633C122A01BCE8E51B10B8B8CB95F580750B3F1(L_4, NULL);
		Transform_tB27202C6F4E36D225EE28A13E4D662BF99785DB1* L_6 = ___2_sourceTransform;
		NullCheck(L_6);
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_7;
		L_7 = Transform_get_localToWorldMatrix_m5D35188766856338DD21DE756F42277C21719E6D(L_6, NULL);
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_8;
		L_8 = Matrix4x4_op_Multiply_m75E91775655DCA8DFC8EDE0AB787285BB3935162_inline(L_5, L_7, NULL);
		V_0 = L_8;
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2* L_9 = ___3_position;
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_10;
		L_10 = Matrix4x4_GetPosition_m9C2F34474967E47C83886A2EC335DFE37B563E80_inline((&V_0), NULL);
		*(Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2*)L_9 = L_10;
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2* L_11 = ___4_eulerAngles;
		Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974 L_12;
		L_12 = Matrix4x4_get_rotation_m7E2C29FCB2AAFAE4D7B4FBD3563E9EDB53F5A8BB_inline((&V_0), NULL);
		V_1 = L_12;
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_13;
		L_13 = Quaternion_get_eulerAngles_m2DB5158B5C3A71FD60FC8A6EE43D3AAA1CFED122_inline((&V_1), NULL);
		*(Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2*)L_11 = L_13;
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2* L_14 = ___5_scale;
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_15;
		L_15 = Matrix4x4_get_lossyScale_mFB3D4CF6408D710D607CA1D2AF94B2E4E0B57EB7_inline((&V_0), NULL);
		*(Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2*)L_14 = L_15;
		return;
	}

IL_0054:
	{
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2* L_16 = ___3_position;
		Transform_tB27202C6F4E36D225EE28A13E4D662BF99785DB1* L_17 = ___2_sourceTransform;
		NullCheck(L_17);
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_18;
		L_18 = Transform_get_position_m69CD5FA214FDAE7BB701552943674846C220FDE1(L_17, NULL);
		*(Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2*)L_16 = L_18;
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2* L_19 = ___4_eulerAngles;
		Transform_tB27202C6F4E36D225EE28A13E4D662BF99785DB1* L_20 = ___2_sourceTransform;
		NullCheck(L_20);
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_21;
		L_21 = Transform_get_eulerAngles_mCAAF48EFCF628F1ED91C2FFE75A4FD19C039DD6A(L_20, NULL);
		*(Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2*)L_19 = L_21;
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2* L_22 = ___5_scale;
		Transform_tB27202C6F4E36D225EE28A13E4D662BF99785DB1* L_23 = ___2_sourceTransform;
		NullCheck(L_23);
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_24;
		L_24 = Transform_get_lossyScale_mFF740DA4BE1489C6882CD2F3A37B7321176E5D07(L_23, NULL);
		*(Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2*)L_22 = L_24;
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 VFXSpaceableBinder_ApplySpacePosition_mA1B55C45A80F225F7FC4562E74E306E90F95D12D (VFXSpaceableBinder_t0C24970B59F1F8E2DA67992F0E90DF951BF91706* __this, VisualEffect_t8CF4F0E13AEBF27224BB1323CC103666EDBBCFB0* ___0_component, ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* ___1_targetProperty, Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 ___2_sourceWorldPosition, const RuntimeMethod* method) 
{
	Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		VisualEffect_t8CF4F0E13AEBF27224BB1323CC103666EDBBCFB0* L_0 = ___0_component;
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_1 = ___1_targetProperty;
		int32_t L_2;
		L_2 = VFXSpaceableBinder_GetTargetSpace_m0137508DC0E097C46C7EAA1ECC89398947D10D58(__this, L_0, L_1, NULL);
		if (L_2)
		{
			goto IL_001f;
		}
	}
	{
		VisualEffect_t8CF4F0E13AEBF27224BB1323CC103666EDBBCFB0* L_3 = ___0_component;
		NullCheck(L_3);
		Transform_tB27202C6F4E36D225EE28A13E4D662BF99785DB1* L_4;
		L_4 = Component_get_transform_m2919A1D81931E6932C7F06D4C2F0AB8DDA9A5371(L_3, NULL);
		NullCheck(L_4);
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_5;
		L_5 = Transform_get_worldToLocalMatrix_mB633C122A01BCE8E51B10B8B8CB95F580750B3F1(L_4, NULL);
		V_0 = L_5;
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_6 = ___2_sourceWorldPosition;
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_7;
		L_7 = Matrix4x4_MultiplyPoint_m20E910B65693559BFDE99382472D8DD02C862E7E_inline((&V_0), L_6, NULL);
		return L_7;
	}

IL_001f:
	{
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_8 = ___2_sourceWorldPosition;
		return L_8;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void VFXSpaceableBinder__ctor_m22AD01FED423E4B640D31DC8DC13AD85422E1A04 (VFXSpaceableBinder_t0C24970B59F1F8E2DA67992F0E90DF951BF91706* __this, const RuntimeMethod* method) 
{
	{
		VFXBinderBase__ctor_m4C0835B8FFF0968BC5378A1D579ABD0168ECF7D3(__this, NULL);
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
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* VFXSphereBinder_get_Property_m4BD037B82D29852C959D5A1FEF723D9A23A00C1F (VFXSphereBinder_t1EC83273D815BCA9C53CD4F7D03C8EAE35811624* __this, const RuntimeMethod* method) 
{
	{
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_0 = __this->___m_Property;
		String_t* L_1;
		L_1 = ExposedProperty_op_Explicit_m8F7C257466E30C5602C4F12AEA2588B7093424F3_inline(L_0, NULL);
		return L_1;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void VFXSphereBinder_set_Property_m2463716FB48677C077197E308EA17763202BAC56 (VFXSphereBinder_t1EC83273D815BCA9C53CD4F7D03C8EAE35811624* __this, String_t* ___0_value, const RuntimeMethod* method) 
{
	{
		String_t* L_0 = ___0_value;
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_1;
		L_1 = ExposedProperty_op_Implicit_m9CEB3D09454745C8E450BAB7C266D15ED28B44FB(L_0, NULL);
		__this->___m_Property = L_1;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___m_Property), (void*)L_1);
		VFXSphereBinder_UpdateSubProperties_mEDEF8CCE8D1B0CDE488F0A113CCF9D3699B4C4A6(__this, NULL);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void VFXSphereBinder_OnEnable_m6B394405C5822497D82D45FD2C688F4D1498C157 (VFXSphereBinder_t1EC83273D815BCA9C53CD4F7D03C8EAE35811624* __this, const RuntimeMethod* method) 
{
	{
		VFXBinderBase_OnEnable_mA026DDFCBD31D8800C70F6A291A8DB6387F2041E(__this, NULL);
		VFXSphereBinder_UpdateSubProperties_mEDEF8CCE8D1B0CDE488F0A113CCF9D3699B4C4A6(__this, NULL);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void VFXSphereBinder_OnValidate_mC678A407430AE4D4EF88B406FDAF3CA847249E33 (VFXSphereBinder_t1EC83273D815BCA9C53CD4F7D03C8EAE35811624* __this, const RuntimeMethod* method) 
{
	{
		VFXSphereBinder_UpdateSubProperties_mEDEF8CCE8D1B0CDE488F0A113CCF9D3699B4C4A6(__this, NULL);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void VFXSphereBinder_UpdateSubProperties_mEDEF8CCE8D1B0CDE488F0A113CCF9D3699B4C4A6 (VFXSphereBinder_t1EC83273D815BCA9C53CD4F7D03C8EAE35811624* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteral7D3C8036CD42ED64DAF6DA744496C82A2C81F94C);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteral95E03FC0AED08252F1928B593F01D7D66A1AB1EF);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteralD6C58774A915D2F4C64ACFD3A7512D0D305BD387);
		s_Il2CppMethodInitialized = true;
	}
	{
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_0 = __this->___m_Property;
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_1;
		L_1 = ExposedProperty_op_Implicit_m9CEB3D09454745C8E450BAB7C266D15ED28B44FB(_stringLiteralD6C58774A915D2F4C64ACFD3A7512D0D305BD387, NULL);
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_2;
		L_2 = ExposedProperty_op_Addition_m2FBBFB0417DEA6D43A730115FA3B9F39A3892C93(L_0, L_1, NULL);
		__this->___m_Old_Center = L_2;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___m_Old_Center), (void*)L_2);
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_3 = __this->___m_Property;
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_4;
		L_4 = ExposedProperty_op_Implicit_m9CEB3D09454745C8E450BAB7C266D15ED28B44FB(_stringLiteral7D3C8036CD42ED64DAF6DA744496C82A2C81F94C, NULL);
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_5;
		L_5 = ExposedProperty_op_Addition_m2FBBFB0417DEA6D43A730115FA3B9F39A3892C93(L_3, L_4, NULL);
		__this->___m_New_Center = L_5;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___m_New_Center), (void*)L_5);
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_6 = __this->___m_Property;
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_7;
		L_7 = ExposedProperty_op_Implicit_m9CEB3D09454745C8E450BAB7C266D15ED28B44FB(_stringLiteral95E03FC0AED08252F1928B593F01D7D66A1AB1EF, NULL);
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_8;
		L_8 = ExposedProperty_op_Addition_m2FBBFB0417DEA6D43A730115FA3B9F39A3892C93(L_6, L_7, NULL);
		__this->___m_Radius = L_8;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___m_Radius), (void*)L_8);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool VFXSphereBinder_IsValid_mDDE887FEA870AF1ED13EDED131F97C76FDCA8332 (VFXSphereBinder_t1EC83273D815BCA9C53CD4F7D03C8EAE35811624* __this, VisualEffect_t8CF4F0E13AEBF27224BB1323CC103666EDBBCFB0* ___0_component, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C_il2cpp_TypeInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	{
		SphereCollider_tBA111C542CE97F6873DE742757213D6265C7D275* L_0 = __this->___Target;
		il2cpp_codegen_runtime_class_init_inline(Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C_il2cpp_TypeInfo_var);
		bool L_1;
		L_1 = Object_op_Inequality_mD0BE578448EAA61948F25C32F8DD55AB1F778602(L_0, (Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C*)NULL, NULL);
		if (!L_1)
		{
			goto IL_0046;
		}
	}
	{
		VisualEffect_t8CF4F0E13AEBF27224BB1323CC103666EDBBCFB0* L_2 = ___0_component;
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_3 = __this->___m_New_Center;
		int32_t L_4;
		L_4 = ExposedProperty_op_Implicit_mA6E7C425842E5FA0B783360A77558C2D45A1970F(L_3, NULL);
		NullCheck(L_2);
		bool L_5;
		L_5 = VisualEffect_HasVector3_mADAFDFDB7950BEF5AFFB2CBD59E620B79F5431C6(L_2, L_4, NULL);
		if (L_5)
		{
			goto IL_0034;
		}
	}
	{
		VisualEffect_t8CF4F0E13AEBF27224BB1323CC103666EDBBCFB0* L_6 = ___0_component;
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_7 = __this->___m_Old_Center;
		int32_t L_8;
		L_8 = ExposedProperty_op_Implicit_mA6E7C425842E5FA0B783360A77558C2D45A1970F(L_7, NULL);
		NullCheck(L_6);
		bool L_9;
		L_9 = VisualEffect_HasVector3_mADAFDFDB7950BEF5AFFB2CBD59E620B79F5431C6(L_6, L_8, NULL);
		if (!L_9)
		{
			goto IL_0046;
		}
	}

IL_0034:
	{
		VisualEffect_t8CF4F0E13AEBF27224BB1323CC103666EDBBCFB0* L_10 = ___0_component;
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_11 = __this->___m_Radius;
		int32_t L_12;
		L_12 = ExposedProperty_op_Implicit_mA6E7C425842E5FA0B783360A77558C2D45A1970F(L_11, NULL);
		NullCheck(L_10);
		bool L_13;
		L_13 = VisualEffect_HasFloat_m9F9969BD960A358735387BB53762466E14D289C6(L_10, L_12, NULL);
		return L_13;
	}

IL_0046:
	{
		return (bool)0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void VFXSphereBinder_UpdateBinding_mC6A1B76D0164FB80EA507DC77B0CBF57729333A0 (VFXSphereBinder_t1EC83273D815BCA9C53CD4F7D03C8EAE35811624* __this, VisualEffect_t8CF4F0E13AEBF27224BB1323CC103666EDBBCFB0* ___0_component, const RuntimeMethod* method) 
{
	ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* V_0 = NULL;
	Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 V_1;
	memset((&V_1), 0, sizeof(V_1));
	Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 V_2;
	memset((&V_2), 0, sizeof(V_2));
	Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 V_3;
	memset((&V_3), 0, sizeof(V_3));
	{
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_0 = __this->___m_New_Center;
		V_0 = L_0;
		VisualEffect_t8CF4F0E13AEBF27224BB1323CC103666EDBBCFB0* L_1 = ___0_component;
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_2 = __this->___m_New_Center;
		int32_t L_3;
		L_3 = ExposedProperty_op_Implicit_mA6E7C425842E5FA0B783360A77558C2D45A1970F(L_2, NULL);
		NullCheck(L_1);
		bool L_4;
		L_4 = VisualEffect_HasVector3_mADAFDFDB7950BEF5AFFB2CBD59E620B79F5431C6(L_1, L_3, NULL);
		if (L_4)
		{
			goto IL_0021;
		}
	}
	{
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_5 = __this->___m_Old_Center;
		V_0 = L_5;
	}

IL_0021:
	{
		VisualEffect_t8CF4F0E13AEBF27224BB1323CC103666EDBBCFB0* L_6 = ___0_component;
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_7 = V_0;
		SphereCollider_tBA111C542CE97F6873DE742757213D6265C7D275* L_8 = __this->___Target;
		NullCheck(L_8);
		Transform_tB27202C6F4E36D225EE28A13E4D662BF99785DB1* L_9;
		L_9 = Component_get_transform_m2919A1D81931E6932C7F06D4C2F0AB8DDA9A5371(L_8, NULL);
		VFXSpaceableBinder_ApplySpaceTS_m3C7E145FA7A6D8B485B8992B7992FA984D76977C(__this, L_6, L_7, L_9, (&V_1), (&V_2), NULL);
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_10 = V_1;
		SphereCollider_tBA111C542CE97F6873DE742757213D6265C7D275* L_11 = __this->___Target;
		NullCheck(L_11);
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_12;
		L_12 = SphereCollider_get_center_m122A197607CD350873539A0EEE3BA10E8BE1759E(L_11, NULL);
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_13;
		L_13 = Vector3_op_Addition_m78C0EC70CB66E8DCAC225743D82B268DAEE92067_inline(L_10, L_12, NULL);
		V_3 = L_13;
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_14 = __this->___m_New_Center;
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_15 = V_0;
		if ((!(((RuntimeObject*)(ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A*)L_14) == ((RuntimeObject*)(ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A*)L_15))))
		{
			goto IL_0067;
		}
	}
	{
		VisualEffect_t8CF4F0E13AEBF27224BB1323CC103666EDBBCFB0* L_16 = ___0_component;
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_17 = __this->___m_New_Center;
		int32_t L_18;
		L_18 = ExposedProperty_op_Implicit_mA6E7C425842E5FA0B783360A77558C2D45A1970F(L_17, NULL);
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_19 = V_3;
		NullCheck(L_16);
		VisualEffect_SetVector3_m9DEA0241E8C6768309AEDC1319F3BD84EAB8D246(L_16, L_18, L_19, NULL);
		goto IL_0079;
	}

IL_0067:
	{
		VisualEffect_t8CF4F0E13AEBF27224BB1323CC103666EDBBCFB0* L_20 = ___0_component;
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_21 = __this->___m_Old_Center;
		int32_t L_22;
		L_22 = ExposedProperty_op_Implicit_mA6E7C425842E5FA0B783360A77558C2D45A1970F(L_21, NULL);
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_23 = V_3;
		NullCheck(L_20);
		VisualEffect_SetVector3_m9DEA0241E8C6768309AEDC1319F3BD84EAB8D246(L_20, L_22, L_23, NULL);
	}

IL_0079:
	{
		VisualEffect_t8CF4F0E13AEBF27224BB1323CC103666EDBBCFB0* L_24 = ___0_component;
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_25 = __this->___m_Radius;
		int32_t L_26;
		L_26 = ExposedProperty_op_Implicit_mA6E7C425842E5FA0B783360A77558C2D45A1970F(L_25, NULL);
		SphereCollider_tBA111C542CE97F6873DE742757213D6265C7D275* L_27 = __this->___Target;
		NullCheck(L_27);
		float L_28;
		L_28 = SphereCollider_get_radius_m1BB513491906E76A4F71929E3DB72A1542309697(L_27, NULL);
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_29 = V_2;
		float L_30;
		L_30 = VFXSphereBinder_GetSphereColliderScale_mFF7014EA91C2D8244BD6F925F0FC267C908696D1(__this, L_29, NULL);
		NullCheck(L_24);
		VisualEffect_SetFloat_m0AF9AD3C2CB14F8004B9F3BE3ECCCA74E3315BA2(L_24, L_26, ((float)il2cpp_codegen_multiply(L_28, L_30)), NULL);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR float VFXSphereBinder_GetSphereColliderScale_mFF7014EA91C2D8244BD6F925F0FC267C908696D1 (VFXSphereBinder_t1EC83273D815BCA9C53CD4F7D03C8EAE35811624* __this, Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 ___0_scale, const RuntimeMethod* method) 
{
	{
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_0 = ___0_scale;
		float L_1 = L_0.___x;
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_2 = ___0_scale;
		float L_3 = L_2.___y;
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_4 = ___0_scale;
		float L_5 = L_4.___z;
		float L_6;
		L_6 = Mathf_Max_mF5379E63D2BBAC76D090748695D833934F8AD051_inline(L_3, L_5, NULL);
		float L_7;
		L_7 = Mathf_Max_mF5379E63D2BBAC76D090748695D833934F8AD051_inline(L_1, L_6, NULL);
		return L_7;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* VFXSphereBinder_ToString_m6E5BAF459D5303CA7A1E0077F558F260605C3BCA (VFXSphereBinder_t1EC83273D815BCA9C53CD4F7D03C8EAE35811624* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteralCDADA8FD6879C24AEC644DDDE1E86A99C7090E2E);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteralE6971243F7EB358A7C6E0D382075D789ADC51C31);
		s_Il2CppMethodInitialized = true;
	}
	ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* G_B2_0 = NULL;
	String_t* G_B2_1 = NULL;
	ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* G_B1_0 = NULL;
	String_t* G_B1_1 = NULL;
	String_t* G_B3_0 = NULL;
	ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* G_B3_1 = NULL;
	String_t* G_B3_2 = NULL;
	{
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_0 = __this->___m_Property;
		SphereCollider_tBA111C542CE97F6873DE742757213D6265C7D275* L_1 = __this->___Target;
		il2cpp_codegen_runtime_class_init_inline(Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C_il2cpp_TypeInfo_var);
		bool L_2;
		L_2 = Object_op_Equality_mB6120F782D83091EF56A198FCEBCF066DB4A9605(L_1, (Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C*)NULL, NULL);
		if (L_2)
		{
			G_B2_0 = L_0;
			G_B2_1 = _stringLiteralCDADA8FD6879C24AEC644DDDE1E86A99C7090E2E;
			goto IL_0026;
		}
		G_B1_0 = L_0;
		G_B1_1 = _stringLiteralCDADA8FD6879C24AEC644DDDE1E86A99C7090E2E;
	}
	{
		SphereCollider_tBA111C542CE97F6873DE742757213D6265C7D275* L_3 = __this->___Target;
		NullCheck(L_3);
		String_t* L_4;
		L_4 = Object_get_name_mAC2F6B897CF1303BA4249B4CB55271AFACBB6392(L_3, NULL);
		G_B3_0 = L_4;
		G_B3_1 = G_B1_0;
		G_B3_2 = G_B1_1;
		goto IL_002b;
	}

IL_0026:
	{
		G_B3_0 = _stringLiteralE6971243F7EB358A7C6E0D382075D789ADC51C31;
		G_B3_1 = G_B2_0;
		G_B3_2 = G_B2_1;
	}

IL_002b:
	{
		String_t* L_5;
		L_5 = String_Format_mFB7DA489BD99F4670881FF50EC017BFB0A5C0987(G_B3_2, G_B3_1, G_B3_0, NULL);
		return L_5;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void VFXSphereBinder__ctor_m69CA4A3B5BF7AAA4F1EAFB16982232AC84FBE3EB (VFXSphereBinder_t1EC83273D815BCA9C53CD4F7D03C8EAE35811624* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteralBDBB8D317096A0BA6BA9463FC2533707873DF8FD);
		s_Il2CppMethodInitialized = true;
	}
	{
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_0;
		L_0 = ExposedProperty_op_Implicit_m9CEB3D09454745C8E450BAB7C266D15ED28B44FB(_stringLiteralBDBB8D317096A0BA6BA9463FC2533707873DF8FD, NULL);
		__this->___m_Property = L_0;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___m_Property), (void*)L_0);
		VFXSpaceableBinder__ctor_m22AD01FED423E4B640D31DC8DC13AD85422E1A04(__this, NULL);
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
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* VFXTerrainBinder_get_Property_m11076487A8F242D0C7569747D702BDEA81F122B8 (VFXTerrainBinder_tBB926D3EE00EA5123B213E37F559F7DDDBFC173F* __this, const RuntimeMethod* method) 
{
	{
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_0 = __this->___m_Property;
		String_t* L_1;
		L_1 = ExposedProperty_op_Explicit_m8F7C257466E30C5602C4F12AEA2588B7093424F3_inline(L_0, NULL);
		return L_1;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void VFXTerrainBinder_set_Property_mFA12D45C44255006830071146EF6E2B71DE5CC10 (VFXTerrainBinder_tBB926D3EE00EA5123B213E37F559F7DDDBFC173F* __this, String_t* ___0_value, const RuntimeMethod* method) 
{
	{
		String_t* L_0 = ___0_value;
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_1;
		L_1 = ExposedProperty_op_Implicit_m9CEB3D09454745C8E450BAB7C266D15ED28B44FB(L_0, NULL);
		__this->___m_Property = L_1;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___m_Property), (void*)L_1);
		VFXTerrainBinder_UpdateSubProperties_m4BC6A8E8C029DD3BD310534ABC857E2EF6FEE282(__this, NULL);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void VFXTerrainBinder_OnEnable_mDFBE56E0010C7316828223EA34F3E72F111F0660 (VFXTerrainBinder_tBB926D3EE00EA5123B213E37F559F7DDDBFC173F* __this, const RuntimeMethod* method) 
{
	{
		VFXBinderBase_OnEnable_mA026DDFCBD31D8800C70F6A291A8DB6387F2041E(__this, NULL);
		VFXTerrainBinder_UpdateSubProperties_m4BC6A8E8C029DD3BD310534ABC857E2EF6FEE282(__this, NULL);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void VFXTerrainBinder_OnValidate_m2792C13EB0EC52B41CA022DE47A2ED676D8F5109 (VFXTerrainBinder_tBB926D3EE00EA5123B213E37F559F7DDDBFC173F* __this, const RuntimeMethod* method) 
{
	{
		VFXTerrainBinder_UpdateSubProperties_m4BC6A8E8C029DD3BD310534ABC857E2EF6FEE282(__this, NULL);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void VFXTerrainBinder_UpdateSubProperties_m4BC6A8E8C029DD3BD310534ABC857E2EF6FEE282 (VFXTerrainBinder_tBB926D3EE00EA5123B213E37F559F7DDDBFC173F* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteral07F8CD0D44E027CC15F7D8BB6A818FF1D2CF8B1C);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteral566B8B38210FC4AA12A6D5CE58BF53C19ADCC217);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteralD0105C238A624817A3CDCE1394508C5E05E41D06);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteralE1126E892A56AB58E8CA367065ED4190ACACBE26);
		s_Il2CppMethodInitialized = true;
	}
	{
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_0 = __this->___m_Property;
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_1;
		L_1 = ExposedProperty_op_Implicit_m9CEB3D09454745C8E450BAB7C266D15ED28B44FB(_stringLiteral566B8B38210FC4AA12A6D5CE58BF53C19ADCC217, NULL);
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_2;
		L_2 = ExposedProperty_op_Addition_m2FBBFB0417DEA6D43A730115FA3B9F39A3892C93(L_0, L_1, NULL);
		__this->___Terrain_Bounds_center = L_2;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___Terrain_Bounds_center), (void*)L_2);
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_3 = __this->___m_Property;
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_4;
		L_4 = ExposedProperty_op_Implicit_m9CEB3D09454745C8E450BAB7C266D15ED28B44FB(_stringLiteralE1126E892A56AB58E8CA367065ED4190ACACBE26, NULL);
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_5;
		L_5 = ExposedProperty_op_Addition_m2FBBFB0417DEA6D43A730115FA3B9F39A3892C93(L_3, L_4, NULL);
		__this->___Terrain_Bounds_size = L_5;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___Terrain_Bounds_size), (void*)L_5);
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_6 = __this->___m_Property;
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_7;
		L_7 = ExposedProperty_op_Implicit_m9CEB3D09454745C8E450BAB7C266D15ED28B44FB(_stringLiteralD0105C238A624817A3CDCE1394508C5E05E41D06, NULL);
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_8;
		L_8 = ExposedProperty_op_Addition_m2FBBFB0417DEA6D43A730115FA3B9F39A3892C93(L_6, L_7, NULL);
		__this->___Terrain_HeightMap = L_8;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___Terrain_HeightMap), (void*)L_8);
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_9 = __this->___m_Property;
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_10;
		L_10 = ExposedProperty_op_Implicit_m9CEB3D09454745C8E450BAB7C266D15ED28B44FB(_stringLiteral07F8CD0D44E027CC15F7D8BB6A818FF1D2CF8B1C, NULL);
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_11;
		L_11 = ExposedProperty_op_Addition_m2FBBFB0417DEA6D43A730115FA3B9F39A3892C93(L_9, L_10, NULL);
		__this->___Terrain_Height = L_11;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___Terrain_Height), (void*)L_11);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool VFXTerrainBinder_IsValid_m9ADEB583B6516C697F5AE5B37BD77BE0FD64897E (VFXTerrainBinder_tBB926D3EE00EA5123B213E37F559F7DDDBFC173F* __this, VisualEffect_t8CF4F0E13AEBF27224BB1323CC103666EDBBCFB0* ___0_component, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C_il2cpp_TypeInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	{
		Terrain_t7F309492F67238DBFBC4566F47385B2A665CF667* L_0 = __this->___Terrain;
		il2cpp_codegen_runtime_class_init_inline(Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C_il2cpp_TypeInfo_var);
		bool L_1;
		L_1 = Object_op_Inequality_mD0BE578448EAA61948F25C32F8DD55AB1F778602(L_0, (Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C*)NULL, NULL);
		if (!L_1)
		{
			goto IL_0059;
		}
	}
	{
		VisualEffect_t8CF4F0E13AEBF27224BB1323CC103666EDBBCFB0* L_2 = ___0_component;
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_3 = __this->___Terrain_Bounds_center;
		int32_t L_4;
		L_4 = ExposedProperty_op_Implicit_mA6E7C425842E5FA0B783360A77558C2D45A1970F(L_3, NULL);
		NullCheck(L_2);
		bool L_5;
		L_5 = VisualEffect_HasVector3_mADAFDFDB7950BEF5AFFB2CBD59E620B79F5431C6(L_2, L_4, NULL);
		if (!L_5)
		{
			goto IL_0059;
		}
	}
	{
		VisualEffect_t8CF4F0E13AEBF27224BB1323CC103666EDBBCFB0* L_6 = ___0_component;
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_7 = __this->___Terrain_Bounds_size;
		int32_t L_8;
		L_8 = ExposedProperty_op_Implicit_mA6E7C425842E5FA0B783360A77558C2D45A1970F(L_7, NULL);
		NullCheck(L_6);
		bool L_9;
		L_9 = VisualEffect_HasVector3_mADAFDFDB7950BEF5AFFB2CBD59E620B79F5431C6(L_6, L_8, NULL);
		if (!L_9)
		{
			goto IL_0059;
		}
	}
	{
		VisualEffect_t8CF4F0E13AEBF27224BB1323CC103666EDBBCFB0* L_10 = ___0_component;
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_11 = __this->___Terrain_HeightMap;
		int32_t L_12;
		L_12 = ExposedProperty_op_Implicit_mA6E7C425842E5FA0B783360A77558C2D45A1970F(L_11, NULL);
		NullCheck(L_10);
		bool L_13;
		L_13 = VisualEffect_HasTexture_m9EE102518FBB3B2ACB6F1DED92638F7FD95AE2FE(L_10, L_12, NULL);
		if (!L_13)
		{
			goto IL_0059;
		}
	}
	{
		VisualEffect_t8CF4F0E13AEBF27224BB1323CC103666EDBBCFB0* L_14 = ___0_component;
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_15 = __this->___Terrain_Height;
		int32_t L_16;
		L_16 = ExposedProperty_op_Implicit_mA6E7C425842E5FA0B783360A77558C2D45A1970F(L_15, NULL);
		NullCheck(L_14);
		bool L_17;
		L_17 = VisualEffect_HasFloat_m9F9969BD960A358735387BB53762466E14D289C6(L_14, L_16, NULL);
		return L_17;
	}

IL_0059:
	{
		return (bool)0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void VFXTerrainBinder_UpdateBinding_m51D293C9821AA974ECCC7D56960952E16146C005 (VFXTerrainBinder_tBB926D3EE00EA5123B213E37F559F7DDDBFC173F* __this, VisualEffect_t8CF4F0E13AEBF27224BB1323CC103666EDBBCFB0* ___0_component, const RuntimeMethod* method) 
{
	Bounds_t367E830C64BBF235ED8C3B2F8CF6254FDCAD39C3 V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		Terrain_t7F309492F67238DBFBC4566F47385B2A665CF667* L_0 = __this->___Terrain;
		NullCheck(L_0);
		TerrainData_t615A68EAC648066681875D47FC641496D12F2E24* L_1;
		L_1 = Terrain_get_terrainData_m3B6C1D89471A4E1C60FC19C168DB37A011B924FD(L_0, NULL);
		NullCheck(L_1);
		Bounds_t367E830C64BBF235ED8C3B2F8CF6254FDCAD39C3 L_2;
		L_2 = TerrainData_get_bounds_m9CE9B3BF067EA06A837AB98B5BC6C0C4669B5A32(L_1, NULL);
		V_0 = L_2;
		VisualEffect_t8CF4F0E13AEBF27224BB1323CC103666EDBBCFB0* L_3 = ___0_component;
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_4 = __this->___Terrain_Bounds_center;
		int32_t L_5;
		L_5 = ExposedProperty_op_Implicit_mA6E7C425842E5FA0B783360A77558C2D45A1970F(L_4, NULL);
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_6;
		L_6 = Bounds_get_center_m5B05F81CB835EB6DD8628FDA24B638F477984DC3_inline((&V_0), NULL);
		NullCheck(L_3);
		VisualEffect_SetVector3_m9DEA0241E8C6768309AEDC1319F3BD84EAB8D246(L_3, L_5, L_6, NULL);
		VisualEffect_t8CF4F0E13AEBF27224BB1323CC103666EDBBCFB0* L_7 = ___0_component;
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_8 = __this->___Terrain_Bounds_size;
		int32_t L_9;
		L_9 = ExposedProperty_op_Implicit_mA6E7C425842E5FA0B783360A77558C2D45A1970F(L_8, NULL);
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_10;
		L_10 = Bounds_get_size_m0699A53A55A78B3201D7270D6F338DFA91B6FAD4_inline((&V_0), NULL);
		NullCheck(L_7);
		VisualEffect_SetVector3_m9DEA0241E8C6768309AEDC1319F3BD84EAB8D246(L_7, L_9, L_10, NULL);
		VisualEffect_t8CF4F0E13AEBF27224BB1323CC103666EDBBCFB0* L_11 = ___0_component;
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_12 = __this->___Terrain_HeightMap;
		int32_t L_13;
		L_13 = ExposedProperty_op_Implicit_mA6E7C425842E5FA0B783360A77558C2D45A1970F(L_12, NULL);
		Terrain_t7F309492F67238DBFBC4566F47385B2A665CF667* L_14 = __this->___Terrain;
		NullCheck(L_14);
		TerrainData_t615A68EAC648066681875D47FC641496D12F2E24* L_15;
		L_15 = Terrain_get_terrainData_m3B6C1D89471A4E1C60FC19C168DB37A011B924FD(L_14, NULL);
		NullCheck(L_15);
		RenderTexture_tBA90C4C3AD9EECCFDDCC632D97C29FAB80D60D27* L_16;
		L_16 = TerrainData_get_heightmapTexture_m26CB863455F0048CEA74741C71BF3021576FF8CE(L_15, NULL);
		NullCheck(L_11);
		VisualEffect_SetTexture_m96338B6E07291B861827B204CAE5654386D91E2C(L_11, L_13, L_16, NULL);
		VisualEffect_t8CF4F0E13AEBF27224BB1323CC103666EDBBCFB0* L_17 = ___0_component;
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_18 = __this->___Terrain_Height;
		int32_t L_19;
		L_19 = ExposedProperty_op_Implicit_mA6E7C425842E5FA0B783360A77558C2D45A1970F(L_18, NULL);
		Terrain_t7F309492F67238DBFBC4566F47385B2A665CF667* L_20 = __this->___Terrain;
		NullCheck(L_20);
		TerrainData_t615A68EAC648066681875D47FC641496D12F2E24* L_21;
		L_21 = Terrain_get_terrainData_m3B6C1D89471A4E1C60FC19C168DB37A011B924FD(L_20, NULL);
		NullCheck(L_21);
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_22;
		L_22 = TerrainData_get_heightmapScale_m4B6AB6495384109BA54955CA52B883A118015188(L_21, NULL);
		float L_23 = L_22.___y;
		NullCheck(L_17);
		VisualEffect_SetFloat_m0AF9AD3C2CB14F8004B9F3BE3ECCCA74E3315BA2(L_17, L_19, L_23, NULL);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* VFXTerrainBinder_ToString_m6BC233FED85CE059387C328F979B8C731467F4EF (VFXTerrainBinder_tBB926D3EE00EA5123B213E37F559F7DDDBFC173F* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteral4FFE0C0087288350842E96EBFBF26816FD08C339);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteralE6971243F7EB358A7C6E0D382075D789ADC51C31);
		s_Il2CppMethodInitialized = true;
	}
	ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* G_B2_0 = NULL;
	String_t* G_B2_1 = NULL;
	ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* G_B1_0 = NULL;
	String_t* G_B1_1 = NULL;
	String_t* G_B3_0 = NULL;
	ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* G_B3_1 = NULL;
	String_t* G_B3_2 = NULL;
	{
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_0 = __this->___m_Property;
		Terrain_t7F309492F67238DBFBC4566F47385B2A665CF667* L_1 = __this->___Terrain;
		il2cpp_codegen_runtime_class_init_inline(Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C_il2cpp_TypeInfo_var);
		bool L_2;
		L_2 = Object_op_Equality_mB6120F782D83091EF56A198FCEBCF066DB4A9605(L_1, (Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C*)NULL, NULL);
		if (L_2)
		{
			G_B2_0 = L_0;
			G_B2_1 = _stringLiteral4FFE0C0087288350842E96EBFBF26816FD08C339;
			goto IL_0026;
		}
		G_B1_0 = L_0;
		G_B1_1 = _stringLiteral4FFE0C0087288350842E96EBFBF26816FD08C339;
	}
	{
		Terrain_t7F309492F67238DBFBC4566F47385B2A665CF667* L_3 = __this->___Terrain;
		NullCheck(L_3);
		String_t* L_4;
		L_4 = Object_get_name_mAC2F6B897CF1303BA4249B4CB55271AFACBB6392(L_3, NULL);
		G_B3_0 = L_4;
		G_B3_1 = G_B1_0;
		G_B3_2 = G_B1_1;
		goto IL_002b;
	}

IL_0026:
	{
		G_B3_0 = _stringLiteralE6971243F7EB358A7C6E0D382075D789ADC51C31;
		G_B3_1 = G_B2_0;
		G_B3_2 = G_B2_1;
	}

IL_002b:
	{
		String_t* L_5;
		L_5 = String_Format_mFB7DA489BD99F4670881FF50EC017BFB0A5C0987(G_B3_2, G_B3_1, G_B3_0, NULL);
		return L_5;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void VFXTerrainBinder__ctor_m1D29723C84DE383CA3430EC4F17495705D5F1A0B (VFXTerrainBinder_tBB926D3EE00EA5123B213E37F559F7DDDBFC173F* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteralE5A22B80916586DE88322B31E1516201F5FE6993);
		s_Il2CppMethodInitialized = true;
	}
	{
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_0;
		L_0 = ExposedProperty_op_Implicit_m9CEB3D09454745C8E450BAB7C266D15ED28B44FB(_stringLiteralE5A22B80916586DE88322B31E1516201F5FE6993, NULL);
		__this->___m_Property = L_0;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___m_Property), (void*)L_0);
		VFXBinderBase__ctor_m4C0835B8FFF0968BC5378A1D579ABD0168ECF7D3(__this, NULL);
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
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* VFXTransformBinder_get_Property_mDDA731FAD1367EF045268409CDED96BFEB180184 (VFXTransformBinder_t2658F13D9C6472FD34C1B8276A298E2F71FE042A* __this, const RuntimeMethod* method) 
{
	{
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_0 = __this->___m_Property;
		String_t* L_1;
		L_1 = ExposedProperty_op_Explicit_m8F7C257466E30C5602C4F12AEA2588B7093424F3_inline(L_0, NULL);
		return L_1;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void VFXTransformBinder_set_Property_m7D20FC04182B2015BFB94C2EFBD33AE32D54F3A7 (VFXTransformBinder_t2658F13D9C6472FD34C1B8276A298E2F71FE042A* __this, String_t* ___0_value, const RuntimeMethod* method) 
{
	{
		String_t* L_0 = ___0_value;
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_1;
		L_1 = ExposedProperty_op_Implicit_m9CEB3D09454745C8E450BAB7C266D15ED28B44FB(L_0, NULL);
		__this->___m_Property = L_1;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___m_Property), (void*)L_1);
		VFXTransformBinder_UpdateSubProperties_mBF83D3F5E36B080C6CA43DF75A6C51B7A8AE6530(__this, NULL);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void VFXTransformBinder_OnEnable_m99FD4C7A36F4D6828151CDA42D7228199F395952 (VFXTransformBinder_t2658F13D9C6472FD34C1B8276A298E2F71FE042A* __this, const RuntimeMethod* method) 
{
	{
		VFXBinderBase_OnEnable_mA026DDFCBD31D8800C70F6A291A8DB6387F2041E(__this, NULL);
		VFXTransformBinder_UpdateSubProperties_mBF83D3F5E36B080C6CA43DF75A6C51B7A8AE6530(__this, NULL);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void VFXTransformBinder_OnValidate_mE7FF581C03E4BAC5D4802770FE8EE7F37B40DFD1 (VFXTransformBinder_t2658F13D9C6472FD34C1B8276A298E2F71FE042A* __this, const RuntimeMethod* method) 
{
	{
		VFXTransformBinder_UpdateSubProperties_mBF83D3F5E36B080C6CA43DF75A6C51B7A8AE6530(__this, NULL);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void VFXTransformBinder_UpdateSubProperties_mBF83D3F5E36B080C6CA43DF75A6C51B7A8AE6530 (VFXTransformBinder_t2658F13D9C6472FD34C1B8276A298E2F71FE042A* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteral06AC4F16B217B88D4CC0899CFE5A559F57DAA082);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteral2C7F0192185C743E1751BE03A4D1C0537E2258D7);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteralCA4FE6DE4D088688B7375BECF5966DF1682F104D);
		s_Il2CppMethodInitialized = true;
	}
	{
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_0 = __this->___m_Property;
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_1;
		L_1 = ExposedProperty_op_Implicit_m9CEB3D09454745C8E450BAB7C266D15ED28B44FB(_stringLiteralCA4FE6DE4D088688B7375BECF5966DF1682F104D, NULL);
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_2;
		L_2 = ExposedProperty_op_Addition_m2FBBFB0417DEA6D43A730115FA3B9F39A3892C93(L_0, L_1, NULL);
		__this->___Position = L_2;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___Position), (void*)L_2);
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_3 = __this->___m_Property;
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_4;
		L_4 = ExposedProperty_op_Implicit_m9CEB3D09454745C8E450BAB7C266D15ED28B44FB(_stringLiteral2C7F0192185C743E1751BE03A4D1C0537E2258D7, NULL);
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_5;
		L_5 = ExposedProperty_op_Addition_m2FBBFB0417DEA6D43A730115FA3B9F39A3892C93(L_3, L_4, NULL);
		__this->___Angles = L_5;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___Angles), (void*)L_5);
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_6 = __this->___m_Property;
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_7;
		L_7 = ExposedProperty_op_Implicit_m9CEB3D09454745C8E450BAB7C266D15ED28B44FB(_stringLiteral06AC4F16B217B88D4CC0899CFE5A559F57DAA082, NULL);
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_8;
		L_8 = ExposedProperty_op_Addition_m2FBBFB0417DEA6D43A730115FA3B9F39A3892C93(L_6, L_7, NULL);
		__this->___Scale = L_8;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___Scale), (void*)L_8);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool VFXTransformBinder_IsValid_m7C1ADF405797D11C4B58B07CDB9E9C42A011319A (VFXTransformBinder_t2658F13D9C6472FD34C1B8276A298E2F71FE042A* __this, VisualEffect_t8CF4F0E13AEBF27224BB1323CC103666EDBBCFB0* ___0_component, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C_il2cpp_TypeInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	{
		Transform_tB27202C6F4E36D225EE28A13E4D662BF99785DB1* L_0 = __this->___Target;
		il2cpp_codegen_runtime_class_init_inline(Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C_il2cpp_TypeInfo_var);
		bool L_1;
		L_1 = Object_op_Inequality_mD0BE578448EAA61948F25C32F8DD55AB1F778602(L_0, (Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C*)NULL, NULL);
		if (!L_1)
		{
			goto IL_0046;
		}
	}
	{
		VisualEffect_t8CF4F0E13AEBF27224BB1323CC103666EDBBCFB0* L_2 = ___0_component;
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_3 = __this->___Position;
		int32_t L_4;
		L_4 = ExposedProperty_op_Implicit_mA6E7C425842E5FA0B783360A77558C2D45A1970F(L_3, NULL);
		NullCheck(L_2);
		bool L_5;
		L_5 = VisualEffect_HasVector3_mADAFDFDB7950BEF5AFFB2CBD59E620B79F5431C6(L_2, L_4, NULL);
		if (!L_5)
		{
			goto IL_0046;
		}
	}
	{
		VisualEffect_t8CF4F0E13AEBF27224BB1323CC103666EDBBCFB0* L_6 = ___0_component;
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_7 = __this->___Angles;
		int32_t L_8;
		L_8 = ExposedProperty_op_Implicit_mA6E7C425842E5FA0B783360A77558C2D45A1970F(L_7, NULL);
		NullCheck(L_6);
		bool L_9;
		L_9 = VisualEffect_HasVector3_mADAFDFDB7950BEF5AFFB2CBD59E620B79F5431C6(L_6, L_8, NULL);
		if (!L_9)
		{
			goto IL_0046;
		}
	}
	{
		VisualEffect_t8CF4F0E13AEBF27224BB1323CC103666EDBBCFB0* L_10 = ___0_component;
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_11 = __this->___Scale;
		int32_t L_12;
		L_12 = ExposedProperty_op_Implicit_mA6E7C425842E5FA0B783360A77558C2D45A1970F(L_11, NULL);
		NullCheck(L_10);
		bool L_13;
		L_13 = VisualEffect_HasVector3_mADAFDFDB7950BEF5AFFB2CBD59E620B79F5431C6(L_10, L_12, NULL);
		return L_13;
	}

IL_0046:
	{
		return (bool)0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void VFXTransformBinder_UpdateBinding_mC78FFF864A6EBCDC4F4BE86912D06B0276DC22A0 (VFXTransformBinder_t2658F13D9C6472FD34C1B8276A298E2F71FE042A* __this, VisualEffect_t8CF4F0E13AEBF27224BB1323CC103666EDBBCFB0* ___0_component, const RuntimeMethod* method) 
{
	Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 V_0;
	memset((&V_0), 0, sizeof(V_0));
	Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 V_1;
	memset((&V_1), 0, sizeof(V_1));
	Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 V_2;
	memset((&V_2), 0, sizeof(V_2));
	{
		VisualEffect_t8CF4F0E13AEBF27224BB1323CC103666EDBBCFB0* L_0 = ___0_component;
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_1 = __this->___Position;
		Transform_tB27202C6F4E36D225EE28A13E4D662BF99785DB1* L_2 = __this->___Target;
		VFXSpaceableBinder_ApplySpaceTRS_m385C4C16AB3E5A84044D1406394ABC9B1A9BB748(__this, L_0, L_1, L_2, (&V_0), (&V_1), (&V_2), NULL);
		VisualEffect_t8CF4F0E13AEBF27224BB1323CC103666EDBBCFB0* L_3 = ___0_component;
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_4 = __this->___Position;
		int32_t L_5;
		L_5 = ExposedProperty_op_Implicit_mA6E7C425842E5FA0B783360A77558C2D45A1970F(L_4, NULL);
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_6 = V_0;
		NullCheck(L_3);
		VisualEffect_SetVector3_m9DEA0241E8C6768309AEDC1319F3BD84EAB8D246(L_3, L_5, L_6, NULL);
		VisualEffect_t8CF4F0E13AEBF27224BB1323CC103666EDBBCFB0* L_7 = ___0_component;
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_8 = __this->___Angles;
		int32_t L_9;
		L_9 = ExposedProperty_op_Implicit_mA6E7C425842E5FA0B783360A77558C2D45A1970F(L_8, NULL);
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_10 = V_1;
		NullCheck(L_7);
		VisualEffect_SetVector3_m9DEA0241E8C6768309AEDC1319F3BD84EAB8D246(L_7, L_9, L_10, NULL);
		VisualEffect_t8CF4F0E13AEBF27224BB1323CC103666EDBBCFB0* L_11 = ___0_component;
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_12 = __this->___Scale;
		int32_t L_13;
		L_13 = ExposedProperty_op_Implicit_mA6E7C425842E5FA0B783360A77558C2D45A1970F(L_12, NULL);
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_14 = V_2;
		NullCheck(L_11);
		VisualEffect_SetVector3_m9DEA0241E8C6768309AEDC1319F3BD84EAB8D246(L_11, L_13, L_14, NULL);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* VFXTransformBinder_ToString_m6446F68FCBEE2476A0B3896C743D125DA37FAC1C (VFXTransformBinder_t2658F13D9C6472FD34C1B8276A298E2F71FE042A* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteralB037A83EE90C5BB4ED7FEF3171A251ADB4E14BE4);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteralE6971243F7EB358A7C6E0D382075D789ADC51C31);
		s_Il2CppMethodInitialized = true;
	}
	ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* G_B2_0 = NULL;
	String_t* G_B2_1 = NULL;
	ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* G_B1_0 = NULL;
	String_t* G_B1_1 = NULL;
	String_t* G_B3_0 = NULL;
	ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* G_B3_1 = NULL;
	String_t* G_B3_2 = NULL;
	{
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_0 = __this->___m_Property;
		Transform_tB27202C6F4E36D225EE28A13E4D662BF99785DB1* L_1 = __this->___Target;
		il2cpp_codegen_runtime_class_init_inline(Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C_il2cpp_TypeInfo_var);
		bool L_2;
		L_2 = Object_op_Equality_mB6120F782D83091EF56A198FCEBCF066DB4A9605(L_1, (Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C*)NULL, NULL);
		if (L_2)
		{
			G_B2_0 = L_0;
			G_B2_1 = _stringLiteralB037A83EE90C5BB4ED7FEF3171A251ADB4E14BE4;
			goto IL_0026;
		}
		G_B1_0 = L_0;
		G_B1_1 = _stringLiteralB037A83EE90C5BB4ED7FEF3171A251ADB4E14BE4;
	}
	{
		Transform_tB27202C6F4E36D225EE28A13E4D662BF99785DB1* L_3 = __this->___Target;
		NullCheck(L_3);
		String_t* L_4;
		L_4 = Object_get_name_mAC2F6B897CF1303BA4249B4CB55271AFACBB6392(L_3, NULL);
		G_B3_0 = L_4;
		G_B3_1 = G_B1_0;
		G_B3_2 = G_B1_1;
		goto IL_002b;
	}

IL_0026:
	{
		G_B3_0 = _stringLiteralE6971243F7EB358A7C6E0D382075D789ADC51C31;
		G_B3_1 = G_B2_0;
		G_B3_2 = G_B2_1;
	}

IL_002b:
	{
		String_t* L_5;
		L_5 = String_Format_mFB7DA489BD99F4670881FF50EC017BFB0A5C0987(G_B3_2, G_B3_1, G_B3_0, NULL);
		return L_5;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void VFXTransformBinder__ctor_mAFB4C41C58EAA855D89425DE74CA282313D31A90 (VFXTransformBinder_t2658F13D9C6472FD34C1B8276A298E2F71FE042A* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteral2766B41C285F5432EBE7E2CE285EDA03E17C0ED9);
		s_Il2CppMethodInitialized = true;
	}
	{
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_0;
		L_0 = ExposedProperty_op_Implicit_m9CEB3D09454745C8E450BAB7C266D15ED28B44FB(_stringLiteral2766B41C285F5432EBE7E2CE285EDA03E17C0ED9, NULL);
		__this->___m_Property = L_0;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___m_Property), (void*)L_0);
		VFXSpaceableBinder__ctor_m22AD01FED423E4B640D31DC8DC13AD85422E1A04(__this, NULL);
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
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* VFXUIDropdownBinder_get_Property_mA312792ECADF9C6FC60D90F817069A49ACCB38C2 (VFXUIDropdownBinder_tE9AA1FDC5FA865CD3412BE969A5A73A8ADA598EE* __this, const RuntimeMethod* method) 
{
	{
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_0 = __this->___m_Property;
		String_t* L_1;
		L_1 = ExposedProperty_op_Explicit_m8F7C257466E30C5602C4F12AEA2588B7093424F3_inline(L_0, NULL);
		return L_1;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void VFXUIDropdownBinder_set_Property_m3ED3BB346E2D25DBE202427337FAC7295D029CF8 (VFXUIDropdownBinder_tE9AA1FDC5FA865CD3412BE969A5A73A8ADA598EE* __this, String_t* ___0_value, const RuntimeMethod* method) 
{
	{
		String_t* L_0 = ___0_value;
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_1;
		L_1 = ExposedProperty_op_Implicit_m9CEB3D09454745C8E450BAB7C266D15ED28B44FB(L_0, NULL);
		__this->___m_Property = L_1;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___m_Property), (void*)L_1);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool VFXUIDropdownBinder_IsValid_m2242EB079F7B03379C451F7E0EE893A05B22CE72 (VFXUIDropdownBinder_tE9AA1FDC5FA865CD3412BE969A5A73A8ADA598EE* __this, VisualEffect_t8CF4F0E13AEBF27224BB1323CC103666EDBBCFB0* ___0_component, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C_il2cpp_TypeInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	{
		Dropdown_t54C0BDC1441E058BE37E796F68886671C270EF89* L_0 = __this->___Target;
		il2cpp_codegen_runtime_class_init_inline(Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C_il2cpp_TypeInfo_var);
		bool L_1;
		L_1 = Object_op_Inequality_mD0BE578448EAA61948F25C32F8DD55AB1F778602(L_0, (Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C*)NULL, NULL);
		if (!L_1)
		{
			goto IL_0020;
		}
	}
	{
		VisualEffect_t8CF4F0E13AEBF27224BB1323CC103666EDBBCFB0* L_2 = ___0_component;
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_3 = __this->___m_Property;
		int32_t L_4;
		L_4 = ExposedProperty_op_Implicit_mA6E7C425842E5FA0B783360A77558C2D45A1970F(L_3, NULL);
		NullCheck(L_2);
		bool L_5;
		L_5 = VisualEffect_HasInt_m1F5851C9BEBD2A887EFF69694465401BDCD94789(L_2, L_4, NULL);
		return L_5;
	}

IL_0020:
	{
		return (bool)0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void VFXUIDropdownBinder_UpdateBinding_mA2D693A069DB8B8E46B94998168B5BA55AE3AAEB (VFXUIDropdownBinder_tE9AA1FDC5FA865CD3412BE969A5A73A8ADA598EE* __this, VisualEffect_t8CF4F0E13AEBF27224BB1323CC103666EDBBCFB0* ___0_component, const RuntimeMethod* method) 
{
	{
		VisualEffect_t8CF4F0E13AEBF27224BB1323CC103666EDBBCFB0* L_0 = ___0_component;
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_1 = __this->___m_Property;
		int32_t L_2;
		L_2 = ExposedProperty_op_Implicit_mA6E7C425842E5FA0B783360A77558C2D45A1970F(L_1, NULL);
		Dropdown_t54C0BDC1441E058BE37E796F68886671C270EF89* L_3 = __this->___Target;
		NullCheck(L_3);
		int32_t L_4;
		L_4 = Dropdown_get_value_m386913162D5E273B762657FE5156DC567602BC3C_inline(L_3, NULL);
		NullCheck(L_0);
		VisualEffect_SetInt_m7F486C2E01DCAACE8D0A43661C8750822CEBD720(L_0, L_2, L_4, NULL);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* VFXUIDropdownBinder_ToString_mD9EB109542CCBE72A2A1B2C0D1ABAF1E888C9525 (VFXUIDropdownBinder_tE9AA1FDC5FA865CD3412BE969A5A73A8ADA598EE* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteral1FC21730ACDFA66F078DA1F77FE2ABE6B0FCE69B);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteralE6971243F7EB358A7C6E0D382075D789ADC51C31);
		s_Il2CppMethodInitialized = true;
	}
	ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* G_B2_0 = NULL;
	String_t* G_B2_1 = NULL;
	ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* G_B1_0 = NULL;
	String_t* G_B1_1 = NULL;
	String_t* G_B3_0 = NULL;
	ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* G_B3_1 = NULL;
	String_t* G_B3_2 = NULL;
	{
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_0 = __this->___m_Property;
		Dropdown_t54C0BDC1441E058BE37E796F68886671C270EF89* L_1 = __this->___Target;
		il2cpp_codegen_runtime_class_init_inline(Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C_il2cpp_TypeInfo_var);
		bool L_2;
		L_2 = Object_op_Equality_mB6120F782D83091EF56A198FCEBCF066DB4A9605(L_1, (Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C*)NULL, NULL);
		if (L_2)
		{
			G_B2_0 = L_0;
			G_B2_1 = _stringLiteral1FC21730ACDFA66F078DA1F77FE2ABE6B0FCE69B;
			goto IL_0026;
		}
		G_B1_0 = L_0;
		G_B1_1 = _stringLiteral1FC21730ACDFA66F078DA1F77FE2ABE6B0FCE69B;
	}
	{
		Dropdown_t54C0BDC1441E058BE37E796F68886671C270EF89* L_3 = __this->___Target;
		NullCheck(L_3);
		String_t* L_4;
		L_4 = Object_get_name_mAC2F6B897CF1303BA4249B4CB55271AFACBB6392(L_3, NULL);
		G_B3_0 = L_4;
		G_B3_1 = G_B1_0;
		G_B3_2 = G_B1_1;
		goto IL_002b;
	}

IL_0026:
	{
		G_B3_0 = _stringLiteralE6971243F7EB358A7C6E0D382075D789ADC51C31;
		G_B3_1 = G_B2_0;
		G_B3_2 = G_B2_1;
	}

IL_002b:
	{
		String_t* L_5;
		L_5 = String_Format_mFB7DA489BD99F4670881FF50EC017BFB0A5C0987(G_B3_2, G_B3_1, G_B3_0, NULL);
		return L_5;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void VFXUIDropdownBinder__ctor_mFE6887A7B2147AB264BD55C93D3276BB962ACF2B (VFXUIDropdownBinder_tE9AA1FDC5FA865CD3412BE969A5A73A8ADA598EE* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteralCA4934A377CA96643EB37C34CCF365B989D5F823);
		s_Il2CppMethodInitialized = true;
	}
	{
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_0;
		L_0 = ExposedProperty_op_Implicit_m9CEB3D09454745C8E450BAB7C266D15ED28B44FB(_stringLiteralCA4934A377CA96643EB37C34CCF365B989D5F823, NULL);
		__this->___m_Property = L_0;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___m_Property), (void*)L_0);
		VFXBinderBase__ctor_m4C0835B8FFF0968BC5378A1D579ABD0168ECF7D3(__this, NULL);
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
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* VFXUISliderBinder_get_Property_m1337B1C947ADE3A5165034AC0CEBEC6BF5E7D6F5 (VFXUISliderBinder_t1A3AF1ABC9C5F1E0AE11E306E67AA2711FFF9516* __this, const RuntimeMethod* method) 
{
	{
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_0 = __this->___m_Property;
		String_t* L_1;
		L_1 = ExposedProperty_op_Explicit_m8F7C257466E30C5602C4F12AEA2588B7093424F3_inline(L_0, NULL);
		return L_1;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void VFXUISliderBinder_set_Property_m64411E8C290FC676ED685978BE0EFA7222703AFC (VFXUISliderBinder_t1A3AF1ABC9C5F1E0AE11E306E67AA2711FFF9516* __this, String_t* ___0_value, const RuntimeMethod* method) 
{
	{
		String_t* L_0 = ___0_value;
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_1;
		L_1 = ExposedProperty_op_Implicit_m9CEB3D09454745C8E450BAB7C266D15ED28B44FB(L_0, NULL);
		__this->___m_Property = L_1;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___m_Property), (void*)L_1);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool VFXUISliderBinder_IsValid_mC9747D1AA84FC9C01A2013D817845BD56FF3AA7A (VFXUISliderBinder_t1A3AF1ABC9C5F1E0AE11E306E67AA2711FFF9516* __this, VisualEffect_t8CF4F0E13AEBF27224BB1323CC103666EDBBCFB0* ___0_component, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C_il2cpp_TypeInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	{
		Slider_t87EA570E3D6556CABF57456C2F3873FFD86E652F* L_0 = __this->___Target;
		il2cpp_codegen_runtime_class_init_inline(Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C_il2cpp_TypeInfo_var);
		bool L_1;
		L_1 = Object_op_Inequality_mD0BE578448EAA61948F25C32F8DD55AB1F778602(L_0, (Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C*)NULL, NULL);
		if (!L_1)
		{
			goto IL_0020;
		}
	}
	{
		VisualEffect_t8CF4F0E13AEBF27224BB1323CC103666EDBBCFB0* L_2 = ___0_component;
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_3 = __this->___m_Property;
		int32_t L_4;
		L_4 = ExposedProperty_op_Implicit_mA6E7C425842E5FA0B783360A77558C2D45A1970F(L_3, NULL);
		NullCheck(L_2);
		bool L_5;
		L_5 = VisualEffect_HasFloat_m9F9969BD960A358735387BB53762466E14D289C6(L_2, L_4, NULL);
		return L_5;
	}

IL_0020:
	{
		return (bool)0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void VFXUISliderBinder_UpdateBinding_mB5228548515D6D956FAF8FF7065DB79450711AF5 (VFXUISliderBinder_t1A3AF1ABC9C5F1E0AE11E306E67AA2711FFF9516* __this, VisualEffect_t8CF4F0E13AEBF27224BB1323CC103666EDBBCFB0* ___0_component, const RuntimeMethod* method) 
{
	{
		VisualEffect_t8CF4F0E13AEBF27224BB1323CC103666EDBBCFB0* L_0 = ___0_component;
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_1 = __this->___m_Property;
		int32_t L_2;
		L_2 = ExposedProperty_op_Implicit_mA6E7C425842E5FA0B783360A77558C2D45A1970F(L_1, NULL);
		Slider_t87EA570E3D6556CABF57456C2F3873FFD86E652F* L_3 = __this->___Target;
		NullCheck(L_3);
		float L_4;
		L_4 = VirtualFuncInvoker0< float >::Invoke(46, L_3);
		NullCheck(L_0);
		VisualEffect_SetFloat_m0AF9AD3C2CB14F8004B9F3BE3ECCCA74E3315BA2(L_0, L_2, L_4, NULL);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* VFXUISliderBinder_ToString_m783D8F7A1C66611D2921F3CADD972A28C6554C92 (VFXUISliderBinder_t1A3AF1ABC9C5F1E0AE11E306E67AA2711FFF9516* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteral302BB8A4174F85F0054649375600275F42150D68);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteralE6971243F7EB358A7C6E0D382075D789ADC51C31);
		s_Il2CppMethodInitialized = true;
	}
	ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* G_B2_0 = NULL;
	String_t* G_B2_1 = NULL;
	ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* G_B1_0 = NULL;
	String_t* G_B1_1 = NULL;
	String_t* G_B3_0 = NULL;
	ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* G_B3_1 = NULL;
	String_t* G_B3_2 = NULL;
	{
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_0 = __this->___m_Property;
		Slider_t87EA570E3D6556CABF57456C2F3873FFD86E652F* L_1 = __this->___Target;
		il2cpp_codegen_runtime_class_init_inline(Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C_il2cpp_TypeInfo_var);
		bool L_2;
		L_2 = Object_op_Equality_mB6120F782D83091EF56A198FCEBCF066DB4A9605(L_1, (Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C*)NULL, NULL);
		if (L_2)
		{
			G_B2_0 = L_0;
			G_B2_1 = _stringLiteral302BB8A4174F85F0054649375600275F42150D68;
			goto IL_0026;
		}
		G_B1_0 = L_0;
		G_B1_1 = _stringLiteral302BB8A4174F85F0054649375600275F42150D68;
	}
	{
		Slider_t87EA570E3D6556CABF57456C2F3873FFD86E652F* L_3 = __this->___Target;
		NullCheck(L_3);
		String_t* L_4;
		L_4 = Object_get_name_mAC2F6B897CF1303BA4249B4CB55271AFACBB6392(L_3, NULL);
		G_B3_0 = L_4;
		G_B3_1 = G_B1_0;
		G_B3_2 = G_B1_1;
		goto IL_002b;
	}

IL_0026:
	{
		G_B3_0 = _stringLiteralE6971243F7EB358A7C6E0D382075D789ADC51C31;
		G_B3_1 = G_B2_0;
		G_B3_2 = G_B2_1;
	}

IL_002b:
	{
		String_t* L_5;
		L_5 = String_Format_mFB7DA489BD99F4670881FF50EC017BFB0A5C0987(G_B3_2, G_B3_1, G_B3_0, NULL);
		return L_5;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void VFXUISliderBinder__ctor_mE12081296E2ADF34102FC1059A762C714EEF2B46 (VFXUISliderBinder_t1A3AF1ABC9C5F1E0AE11E306E67AA2711FFF9516* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteral8C84BD48DC65AFD12F768ECC14BFBB353EC1DA63);
		s_Il2CppMethodInitialized = true;
	}
	{
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_0;
		L_0 = ExposedProperty_op_Implicit_m9CEB3D09454745C8E450BAB7C266D15ED28B44FB(_stringLiteral8C84BD48DC65AFD12F768ECC14BFBB353EC1DA63, NULL);
		__this->___m_Property = L_0;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___m_Property), (void*)L_0);
		VFXBinderBase__ctor_m4C0835B8FFF0968BC5378A1D579ABD0168ECF7D3(__this, NULL);
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
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* VFXUIToggleBinder_get_Property_mD8909CE48307CA7F8218CFFFABE4478B4B76FAED (VFXUIToggleBinder_t16637477616EDAD883FC73354821CFF850463E1E* __this, const RuntimeMethod* method) 
{
	{
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_0 = __this->___m_Property;
		String_t* L_1;
		L_1 = ExposedProperty_op_Explicit_m8F7C257466E30C5602C4F12AEA2588B7093424F3_inline(L_0, NULL);
		return L_1;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void VFXUIToggleBinder_set_Property_m0D1AD5FC6784FDD9B02A50E4ECF627A46082BC7E (VFXUIToggleBinder_t16637477616EDAD883FC73354821CFF850463E1E* __this, String_t* ___0_value, const RuntimeMethod* method) 
{
	{
		String_t* L_0 = ___0_value;
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_1;
		L_1 = ExposedProperty_op_Implicit_m9CEB3D09454745C8E450BAB7C266D15ED28B44FB(L_0, NULL);
		__this->___m_Property = L_1;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___m_Property), (void*)L_1);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool VFXUIToggleBinder_IsValid_m8CE38454205A6E0CAE24F652D990B61115A4AA59 (VFXUIToggleBinder_t16637477616EDAD883FC73354821CFF850463E1E* __this, VisualEffect_t8CF4F0E13AEBF27224BB1323CC103666EDBBCFB0* ___0_component, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C_il2cpp_TypeInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	{
		Toggle_tBF13F3EBA485E06826FD8A38F4B4C1380DF21A1F* L_0 = __this->___Target;
		il2cpp_codegen_runtime_class_init_inline(Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C_il2cpp_TypeInfo_var);
		bool L_1;
		L_1 = Object_op_Inequality_mD0BE578448EAA61948F25C32F8DD55AB1F778602(L_0, (Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C*)NULL, NULL);
		if (!L_1)
		{
			goto IL_0020;
		}
	}
	{
		VisualEffect_t8CF4F0E13AEBF27224BB1323CC103666EDBBCFB0* L_2 = ___0_component;
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_3 = __this->___m_Property;
		int32_t L_4;
		L_4 = ExposedProperty_op_Implicit_mA6E7C425842E5FA0B783360A77558C2D45A1970F(L_3, NULL);
		NullCheck(L_2);
		bool L_5;
		L_5 = VisualEffect_HasBool_m880EB204D4723EB3C06B2E34A52486E678A752AA(L_2, L_4, NULL);
		return L_5;
	}

IL_0020:
	{
		return (bool)0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void VFXUIToggleBinder_UpdateBinding_m540D27870616173E0F74275B324162E132EDDE44 (VFXUIToggleBinder_t16637477616EDAD883FC73354821CFF850463E1E* __this, VisualEffect_t8CF4F0E13AEBF27224BB1323CC103666EDBBCFB0* ___0_component, const RuntimeMethod* method) 
{
	{
		VisualEffect_t8CF4F0E13AEBF27224BB1323CC103666EDBBCFB0* L_0 = ___0_component;
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_1 = __this->___m_Property;
		int32_t L_2;
		L_2 = ExposedProperty_op_Implicit_mA6E7C425842E5FA0B783360A77558C2D45A1970F(L_1, NULL);
		Toggle_tBF13F3EBA485E06826FD8A38F4B4C1380DF21A1F* L_3 = __this->___Target;
		NullCheck(L_3);
		bool L_4;
		L_4 = Toggle_get_isOn_m89A609E936CD67F460E336CA8E03C4047BFB6619_inline(L_3, NULL);
		NullCheck(L_0);
		VisualEffect_SetBool_m835964F6BF4751B1B517E03F3676552164E7927D(L_0, L_2, L_4, NULL);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* VFXUIToggleBinder_ToString_m5F9673F0E3DB2AD4BB1EF915F3FE1545E5D08F23 (VFXUIToggleBinder_t16637477616EDAD883FC73354821CFF850463E1E* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteral5258D5F30E97A1EA5F294DD45A880DA0FCE2FC03);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteralE6971243F7EB358A7C6E0D382075D789ADC51C31);
		s_Il2CppMethodInitialized = true;
	}
	ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* G_B2_0 = NULL;
	String_t* G_B2_1 = NULL;
	ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* G_B1_0 = NULL;
	String_t* G_B1_1 = NULL;
	String_t* G_B3_0 = NULL;
	ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* G_B3_1 = NULL;
	String_t* G_B3_2 = NULL;
	{
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_0 = __this->___m_Property;
		Toggle_tBF13F3EBA485E06826FD8A38F4B4C1380DF21A1F* L_1 = __this->___Target;
		il2cpp_codegen_runtime_class_init_inline(Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C_il2cpp_TypeInfo_var);
		bool L_2;
		L_2 = Object_op_Equality_mB6120F782D83091EF56A198FCEBCF066DB4A9605(L_1, (Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C*)NULL, NULL);
		if (L_2)
		{
			G_B2_0 = L_0;
			G_B2_1 = _stringLiteral5258D5F30E97A1EA5F294DD45A880DA0FCE2FC03;
			goto IL_0026;
		}
		G_B1_0 = L_0;
		G_B1_1 = _stringLiteral5258D5F30E97A1EA5F294DD45A880DA0FCE2FC03;
	}
	{
		Toggle_tBF13F3EBA485E06826FD8A38F4B4C1380DF21A1F* L_3 = __this->___Target;
		NullCheck(L_3);
		String_t* L_4;
		L_4 = Object_get_name_mAC2F6B897CF1303BA4249B4CB55271AFACBB6392(L_3, NULL);
		G_B3_0 = L_4;
		G_B3_1 = G_B1_0;
		G_B3_2 = G_B1_1;
		goto IL_002b;
	}

IL_0026:
	{
		G_B3_0 = _stringLiteralE6971243F7EB358A7C6E0D382075D789ADC51C31;
		G_B3_1 = G_B2_0;
		G_B3_2 = G_B2_1;
	}

IL_002b:
	{
		String_t* L_5;
		L_5 = String_Format_mFB7DA489BD99F4670881FF50EC017BFB0A5C0987(G_B3_2, G_B3_1, G_B3_0, NULL);
		return L_5;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void VFXUIToggleBinder__ctor_mF32CC67443A5FCD342046325703CB346AAA66811 (VFXUIToggleBinder_t16637477616EDAD883FC73354821CFF850463E1E* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteral880249E7973673F42C9C8FC573E4CC1ECC6758F3);
		s_Il2CppMethodInitialized = true;
	}
	{
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_0;
		L_0 = ExposedProperty_op_Implicit_m9CEB3D09454745C8E450BAB7C266D15ED28B44FB(_stringLiteral880249E7973673F42C9C8FC573E4CC1ECC6758F3, NULL);
		__this->___m_Property = L_0;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___m_Property), (void*)L_0);
		VFXBinderBase__ctor_m4C0835B8FFF0968BC5378A1D579ABD0168ECF7D3(__this, NULL);
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
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* VFXVelocityBinder_get_Property_m1A1D476AFF3A3D71861361F157C3648BE4E13376 (VFXVelocityBinder_t70DB0B43C2865624A6C9127476F12BC5B6EA31FC* __this, const RuntimeMethod* method) 
{
	{
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_0 = __this->___m_Property;
		String_t* L_1;
		L_1 = ExposedProperty_op_Explicit_m8F7C257466E30C5602C4F12AEA2588B7093424F3_inline(L_0, NULL);
		return L_1;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void VFXVelocityBinder_set_Property_m962BAC85948E45D56FD6AC427AF4644617906847 (VFXVelocityBinder_t70DB0B43C2865624A6C9127476F12BC5B6EA31FC* __this, String_t* ___0_value, const RuntimeMethod* method) 
{
	{
		String_t* L_0 = ___0_value;
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_1;
		L_1 = ExposedProperty_op_Implicit_m9CEB3D09454745C8E450BAB7C266D15ED28B44FB(L_0, NULL);
		__this->___m_Property = L_1;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___m_Property), (void*)L_1);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool VFXVelocityBinder_IsValid_m733A7FFC62AE73C5EC2718C579318EEE889A6BB2 (VFXVelocityBinder_t70DB0B43C2865624A6C9127476F12BC5B6EA31FC* __this, VisualEffect_t8CF4F0E13AEBF27224BB1323CC103666EDBBCFB0* ___0_component, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C_il2cpp_TypeInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	{
		Transform_tB27202C6F4E36D225EE28A13E4D662BF99785DB1* L_0 = __this->___Target;
		il2cpp_codegen_runtime_class_init_inline(Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C_il2cpp_TypeInfo_var);
		bool L_1;
		L_1 = Object_op_Inequality_mD0BE578448EAA61948F25C32F8DD55AB1F778602(L_0, (Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C*)NULL, NULL);
		if (!L_1)
		{
			goto IL_0020;
		}
	}
	{
		VisualEffect_t8CF4F0E13AEBF27224BB1323CC103666EDBBCFB0* L_2 = ___0_component;
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_3 = __this->___m_Property;
		int32_t L_4;
		L_4 = ExposedProperty_op_Implicit_mA6E7C425842E5FA0B783360A77558C2D45A1970F(L_3, NULL);
		NullCheck(L_2);
		bool L_5;
		L_5 = VisualEffect_HasVector3_mADAFDFDB7950BEF5AFFB2CBD59E620B79F5431C6(L_2, L_4, NULL);
		return L_5;
	}

IL_0020:
	{
		return (bool)0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void VFXVelocityBinder_Reset_mDACCE514C2B72E4B47F809BCDD65B976A7F232CD (VFXVelocityBinder_t70DB0B43C2865624A6C9127476F12BC5B6EA31FC* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&VFXVelocityBinder_t70DB0B43C2865624A6C9127476F12BC5B6EA31FC_il2cpp_TypeInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	{
		il2cpp_codegen_runtime_class_init_inline(VFXVelocityBinder_t70DB0B43C2865624A6C9127476F12BC5B6EA31FC_il2cpp_TypeInfo_var);
		float L_0 = ((VFXVelocityBinder_t70DB0B43C2865624A6C9127476F12BC5B6EA31FC_StaticFields*)il2cpp_codegen_static_fields_for(VFXVelocityBinder_t70DB0B43C2865624A6C9127476F12BC5B6EA31FC_il2cpp_TypeInfo_var))->___invalidPreviousTime;
		__this->___m_PreviousTime = L_0;
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void VFXVelocityBinder_UpdateBinding_m09ED97F0F480345359CA957026989E7E020D3FDD (VFXVelocityBinder_t70DB0B43C2865624A6C9127476F12BC5B6EA31FC* __this, VisualEffect_t8CF4F0E13AEBF27224BB1323CC103666EDBBCFB0* ___0_component, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Mathf_tE284D016E3B297B72311AAD9EB8F0E643F6A4682_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&VFXVelocityBinder_t70DB0B43C2865624A6C9127476F12BC5B6EA31FC_il2cpp_TypeInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 V_0;
	memset((&V_0), 0, sizeof(V_0));
	float V_1 = 0.0f;
	Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 V_2;
	memset((&V_2), 0, sizeof(V_2));
	Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 V_3;
	memset((&V_3), 0, sizeof(V_3));
	float V_4 = 0.0f;
	{
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_0;
		L_0 = Vector3_get_zero_m0C1249C3F25B1C70EAD3CC8B31259975A457AE39_inline(NULL);
		V_0 = L_0;
		float L_1;
		L_1 = Time_get_time_m3A271BB1B20041144AC5B7863B71AB1F0150374B(NULL);
		V_1 = L_1;
		VisualEffect_t8CF4F0E13AEBF27224BB1323CC103666EDBBCFB0* L_2 = ___0_component;
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_3 = __this->___m_Property;
		Transform_tB27202C6F4E36D225EE28A13E4D662BF99785DB1* L_4 = __this->___Target;
		NullCheck(L_4);
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_5;
		L_5 = Transform_get_position_m69CD5FA214FDAE7BB701552943674846C220FDE1(L_4, NULL);
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_6;
		L_6 = VFXSpaceableBinder_ApplySpacePosition_mA1B55C45A80F225F7FC4562E74E306E90F95D12D(__this, L_2, L_3, L_5, NULL);
		V_2 = L_6;
		float L_7 = __this->___m_PreviousTime;
		il2cpp_codegen_runtime_class_init_inline(VFXVelocityBinder_t70DB0B43C2865624A6C9127476F12BC5B6EA31FC_il2cpp_TypeInfo_var);
		float L_8 = ((VFXVelocityBinder_t70DB0B43C2865624A6C9127476F12BC5B6EA31FC_StaticFields*)il2cpp_codegen_static_fields_for(VFXVelocityBinder_t70DB0B43C2865624A6C9127476F12BC5B6EA31FC_il2cpp_TypeInfo_var))->___invalidPreviousTime;
		if ((((float)L_7) == ((float)L_8)))
		{
			goto IL_0068;
		}
	}
	{
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_9 = V_2;
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_10 = __this->___m_PreviousPosition;
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_11;
		L_11 = Vector3_op_Subtraction_mE42023FF80067CB44A1D4A27EB7CF2B24CABB828_inline(L_9, L_10, NULL);
		V_3 = L_11;
		float L_12 = V_1;
		float L_13 = __this->___m_PreviousTime;
		V_4 = ((float)il2cpp_codegen_subtract(L_12, L_13));
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_14 = V_3;
		float L_15;
		L_15 = Vector3_SqrMagnitude_m5ED73273031577902F1BED4CA9C5A0E786A680D1_inline(L_14, NULL);
		float L_16 = ((Mathf_tE284D016E3B297B72311AAD9EB8F0E643F6A4682_StaticFields*)il2cpp_codegen_static_fields_for(Mathf_tE284D016E3B297B72311AAD9EB8F0E643F6A4682_il2cpp_TypeInfo_var))->___Epsilon;
		if ((!(((float)L_15) > ((float)L_16))))
		{
			goto IL_0068;
		}
	}
	{
		float L_17 = V_4;
		float L_18 = ((Mathf_tE284D016E3B297B72311AAD9EB8F0E643F6A4682_StaticFields*)il2cpp_codegen_static_fields_for(Mathf_tE284D016E3B297B72311AAD9EB8F0E643F6A4682_il2cpp_TypeInfo_var))->___Epsilon;
		if ((!(((float)L_17) > ((float)L_18))))
		{
			goto IL_0068;
		}
	}
	{
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_19 = V_3;
		float L_20 = V_4;
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_21;
		L_21 = Vector3_op_Division_mCC6BB24E372AB96B8380D1678446EF6A8BAE13BB_inline(L_19, L_20, NULL);
		V_0 = L_21;
	}

IL_0068:
	{
		VisualEffect_t8CF4F0E13AEBF27224BB1323CC103666EDBBCFB0* L_22 = ___0_component;
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_23 = __this->___m_Property;
		int32_t L_24;
		L_24 = ExposedProperty_op_Implicit_mA6E7C425842E5FA0B783360A77558C2D45A1970F(L_23, NULL);
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_25 = V_0;
		NullCheck(L_22);
		VisualEffect_SetVector3_m9DEA0241E8C6768309AEDC1319F3BD84EAB8D246(L_22, L_24, L_25, NULL);
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_26 = V_2;
		__this->___m_PreviousPosition = L_26;
		float L_27 = V_1;
		__this->___m_PreviousTime = L_27;
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* VFXVelocityBinder_ToString_m1F67119485CFD21DB8903023EB78BB7D7123332F (VFXVelocityBinder_t70DB0B43C2865624A6C9127476F12BC5B6EA31FC* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteral82D26207184485AEC867969F0F1F452A4ADA97A6);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteralE6971243F7EB358A7C6E0D382075D789ADC51C31);
		s_Il2CppMethodInitialized = true;
	}
	ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* G_B2_0 = NULL;
	String_t* G_B2_1 = NULL;
	ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* G_B1_0 = NULL;
	String_t* G_B1_1 = NULL;
	String_t* G_B3_0 = NULL;
	ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* G_B3_1 = NULL;
	String_t* G_B3_2 = NULL;
	{
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_0 = __this->___m_Property;
		Transform_tB27202C6F4E36D225EE28A13E4D662BF99785DB1* L_1 = __this->___Target;
		il2cpp_codegen_runtime_class_init_inline(Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C_il2cpp_TypeInfo_var);
		bool L_2;
		L_2 = Object_op_Equality_mB6120F782D83091EF56A198FCEBCF066DB4A9605(L_1, (Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C*)NULL, NULL);
		if (L_2)
		{
			G_B2_0 = L_0;
			G_B2_1 = _stringLiteral82D26207184485AEC867969F0F1F452A4ADA97A6;
			goto IL_0026;
		}
		G_B1_0 = L_0;
		G_B1_1 = _stringLiteral82D26207184485AEC867969F0F1F452A4ADA97A6;
	}
	{
		Transform_tB27202C6F4E36D225EE28A13E4D662BF99785DB1* L_3 = __this->___Target;
		NullCheck(L_3);
		String_t* L_4;
		L_4 = Object_get_name_mAC2F6B897CF1303BA4249B4CB55271AFACBB6392(L_3, NULL);
		G_B3_0 = L_4;
		G_B3_1 = G_B1_0;
		G_B3_2 = G_B1_1;
		goto IL_002b;
	}

IL_0026:
	{
		G_B3_0 = _stringLiteralE6971243F7EB358A7C6E0D382075D789ADC51C31;
		G_B3_1 = G_B2_0;
		G_B3_2 = G_B2_1;
	}

IL_002b:
	{
		String_t* L_5;
		L_5 = String_Format_mFB7DA489BD99F4670881FF50EC017BFB0A5C0987(G_B3_2, G_B3_1, G_B3_0, NULL);
		return L_5;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void VFXVelocityBinder__ctor_m90ADA6EBDCAF56890C4078630ED7E2FF1519596B (VFXVelocityBinder_t70DB0B43C2865624A6C9127476F12BC5B6EA31FC* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&VFXVelocityBinder_t70DB0B43C2865624A6C9127476F12BC5B6EA31FC_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteralB0E010F888B7DEEF0B2CD01DD03D8C9A96B9AAFB);
		s_Il2CppMethodInitialized = true;
	}
	{
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_0;
		L_0 = ExposedProperty_op_Implicit_m9CEB3D09454745C8E450BAB7C266D15ED28B44FB(_stringLiteralB0E010F888B7DEEF0B2CD01DD03D8C9A96B9AAFB, NULL);
		__this->___m_Property = L_0;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___m_Property), (void*)L_0);
		il2cpp_codegen_runtime_class_init_inline(VFXVelocityBinder_t70DB0B43C2865624A6C9127476F12BC5B6EA31FC_il2cpp_TypeInfo_var);
		float L_1 = ((VFXVelocityBinder_t70DB0B43C2865624A6C9127476F12BC5B6EA31FC_StaticFields*)il2cpp_codegen_static_fields_for(VFXVelocityBinder_t70DB0B43C2865624A6C9127476F12BC5B6EA31FC_il2cpp_TypeInfo_var))->___invalidPreviousTime;
		__this->___m_PreviousTime = L_1;
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_2;
		L_2 = Vector3_get_zero_m0C1249C3F25B1C70EAD3CC8B31259975A457AE39_inline(NULL);
		__this->___m_PreviousPosition = L_2;
		VFXSpaceableBinder__ctor_m22AD01FED423E4B640D31DC8DC13AD85422E1A04(__this, NULL);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void VFXVelocityBinder__cctor_m1DCF8B74965367AADAED91267964D319EB6C532F (const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&VFXVelocityBinder_t70DB0B43C2865624A6C9127476F12BC5B6EA31FC_il2cpp_TypeInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	{
		((VFXVelocityBinder_t70DB0B43C2865624A6C9127476F12BC5B6EA31FC_StaticFields*)il2cpp_codegen_static_fields_for(VFXVelocityBinder_t70DB0B43C2865624A6C9127476F12BC5B6EA31FC_il2cpp_TypeInfo_var))->___invalidPreviousTime = (-1.0f);
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
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void VFXBinderAttribute__ctor_m5685F22B447E9F6F9F0AD314B115F9ACAB27EED1 (VFXBinderAttribute_tB68026C86370BE191A6C1F1DD6002F89B7625E0B* __this, String_t* ___0_menuPath, const RuntimeMethod* method) 
{
	{
		PropertyAttribute__ctor_m19247686E165101F140615C7306DC2DA3953D97D(__this, NULL);
		String_t* L_0 = ___0_menuPath;
		__this->___MenuPath = L_0;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___MenuPath), (void*)L_0);
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
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void VFXBinderBase_Reset_m7C64FFDCB71B077590384C512E0776B33B23477E (VFXBinderBase_t8BC197F500B3A144DF76E77C48FA239C04CDA19C* __this, const RuntimeMethod* method) 
{
	{
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void VFXBinderBase_Awake_mB78C136536D663C6188EA4575AC9F971F512B915 (VFXBinderBase_t8BC197F500B3A144DF76E77C48FA239C04CDA19C* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Component_GetComponent_TisVFXPropertyBinder_t761F7FEC5308F4D9ABF7F0A5A3FAAAA6FE043C0F_m483C444935A49F5B275955F30616CCD6440760CA_RuntimeMethod_var);
		s_Il2CppMethodInitialized = true;
	}
	{
		VFXPropertyBinder_t761F7FEC5308F4D9ABF7F0A5A3FAAAA6FE043C0F* L_0;
		L_0 = Component_GetComponent_TisVFXPropertyBinder_t761F7FEC5308F4D9ABF7F0A5A3FAAAA6FE043C0F_m483C444935A49F5B275955F30616CCD6440760CA(__this, Component_GetComponent_TisVFXPropertyBinder_t761F7FEC5308F4D9ABF7F0A5A3FAAAA6FE043C0F_m483C444935A49F5B275955F30616CCD6440760CA_RuntimeMethod_var);
		__this->___binder = L_0;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___binder), (void*)L_0);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void VFXBinderBase_OnEnable_mA026DDFCBD31D8800C70F6A291A8DB6387F2041E (VFXBinderBase_t8BC197F500B3A144DF76E77C48FA239C04CDA19C* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&List_1_Add_m795E062030C3253617A08E869C6D58DF7FEC6799_RuntimeMethod_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&List_1_Contains_mEB538ED5525355D6EF6699D85A92560EDA91A11F_RuntimeMethod_var);
		s_Il2CppMethodInitialized = true;
	}
	{
		VFXPropertyBinder_t761F7FEC5308F4D9ABF7F0A5A3FAAAA6FE043C0F* L_0 = __this->___binder;
		NullCheck(L_0);
		List_1_t5A6730315AA46B39985DF97B343C4E4ACEF77B42* L_1 = L_0->___m_Bindings;
		NullCheck(L_1);
		bool L_2;
		L_2 = List_1_Contains_mEB538ED5525355D6EF6699D85A92560EDA91A11F(L_1, __this, List_1_Contains_mEB538ED5525355D6EF6699D85A92560EDA91A11F_RuntimeMethod_var);
		if (L_2)
		{
			goto IL_0024;
		}
	}
	{
		VFXPropertyBinder_t761F7FEC5308F4D9ABF7F0A5A3FAAAA6FE043C0F* L_3 = __this->___binder;
		NullCheck(L_3);
		List_1_t5A6730315AA46B39985DF97B343C4E4ACEF77B42* L_4 = L_3->___m_Bindings;
		NullCheck(L_4);
		List_1_Add_m795E062030C3253617A08E869C6D58DF7FEC6799_inline(L_4, __this, List_1_Add_m795E062030C3253617A08E869C6D58DF7FEC6799_RuntimeMethod_var);
	}

IL_0024:
	{
		Object_set_hideFlags_mACB8BFC903FB3B01BBD427753E791BF28B5E33D4(__this, 2, NULL);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void VFXBinderBase_OnDisable_mC6D17B49F6277356AD6F67652EE80B122920B577 (VFXBinderBase_t8BC197F500B3A144DF76E77C48FA239C04CDA19C* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&List_1_Contains_mEB538ED5525355D6EF6699D85A92560EDA91A11F_RuntimeMethod_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&List_1_Remove_mB0623D1E0568D9930B5F47623B7220C2F47B6CB1_RuntimeMethod_var);
		s_Il2CppMethodInitialized = true;
	}
	{
		VFXPropertyBinder_t761F7FEC5308F4D9ABF7F0A5A3FAAAA6FE043C0F* L_0 = __this->___binder;
		NullCheck(L_0);
		List_1_t5A6730315AA46B39985DF97B343C4E4ACEF77B42* L_1 = L_0->___m_Bindings;
		NullCheck(L_1);
		bool L_2;
		L_2 = List_1_Contains_mEB538ED5525355D6EF6699D85A92560EDA91A11F(L_1, __this, List_1_Contains_mEB538ED5525355D6EF6699D85A92560EDA91A11F_RuntimeMethod_var);
		if (!L_2)
		{
			goto IL_0025;
		}
	}
	{
		VFXPropertyBinder_t761F7FEC5308F4D9ABF7F0A5A3FAAAA6FE043C0F* L_3 = __this->___binder;
		NullCheck(L_3);
		List_1_t5A6730315AA46B39985DF97B343C4E4ACEF77B42* L_4 = L_3->___m_Bindings;
		NullCheck(L_4);
		bool L_5;
		L_5 = List_1_Remove_mB0623D1E0568D9930B5F47623B7220C2F47B6CB1(L_4, __this, List_1_Remove_mB0623D1E0568D9930B5F47623B7220C2F47B6CB1_RuntimeMethod_var);
	}

IL_0025:
	{
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* VFXBinderBase_ToString_m7FA1FF8BC36F8140D98A49BEF82EA8CCAF2E8DB0 (VFXBinderBase_t8BC197F500B3A144DF76E77C48FA239C04CDA19C* __this, const RuntimeMethod* method) 
{
	{
		Type_t* L_0;
		L_0 = Object_GetType_mE10A8FC1E57F3DF29972CCBC026C2DC3942263B3(__this, NULL);
		NullCheck(L_0);
		String_t* L_1;
		L_1 = VirtualFuncInvoker0< String_t* >::Invoke(3, L_0);
		return L_1;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void VFXBinderBase__ctor_m4C0835B8FFF0968BC5378A1D579ABD0168ECF7D3 (VFXBinderBase_t8BC197F500B3A144DF76E77C48FA239C04CDA19C* __this, const RuntimeMethod* method) 
{
	{
		MonoBehaviour__ctor_m592DB0105CA0BC97AA1C5F4AD27B12D68A3B7C1E(__this, NULL);
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
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void VFXPropertyBinder_OnEnable_m867A5FEA5EDE524F0F7DF47EB86BF80128A6F298 (VFXPropertyBinder_t761F7FEC5308F4D9ABF7F0A5A3FAAAA6FE043C0F* __this, const RuntimeMethod* method) 
{
	{
		VFXPropertyBinder_Reload_mC20E8C114F88956652BCFE1D1AB7701BA946CA4A(__this, NULL);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void VFXPropertyBinder_OnValidate_m31AF250AFDCBBEAEEA73D29AF8793E25CEDF478D (VFXPropertyBinder_t761F7FEC5308F4D9ABF7F0A5A3FAAAA6FE043C0F* __this, const RuntimeMethod* method) 
{
	{
		VFXPropertyBinder_Reload_mC20E8C114F88956652BCFE1D1AB7701BA946CA4A(__this, NULL);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void VFXPropertyBinder_SafeDestroy_m4BC4B6A481989E373832AB7449F231ACB640F1AA (Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C* ___0_toDelete, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C_il2cpp_TypeInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	{
		Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C* L_0 = ___0_toDelete;
		il2cpp_codegen_runtime_class_init_inline(Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C_il2cpp_TypeInfo_var);
		Object_Destroy_mE97D0A766419A81296E8D4E5C23D01D3FE91ACBB(L_0, NULL);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void VFXPropertyBinder_Reload_mC20E8C114F88956652BCFE1D1AB7701BA946CA4A (VFXPropertyBinder_t761F7FEC5308F4D9ABF7F0A5A3FAAAA6FE043C0F* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Component_GetComponent_TisVisualEffect_t8CF4F0E13AEBF27224BB1323CC103666EDBBCFB0_mC2DD444DE804211C8EADF53E224C7103F9A3CCD1_RuntimeMethod_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&GameObject_GetComponents_TisVFXBinderBase_t8BC197F500B3A144DF76E77C48FA239C04CDA19C_m2849BF084DFC48ABA7745E3C7EC2249159F03323_RuntimeMethod_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&List_1_AddRange_mD0480735EA168951277C2C2EB3F55E76699E8795_RuntimeMethod_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&List_1__ctor_m18A0E45DF3874B841F030036ABD1CBDCF7D77555_RuntimeMethod_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&List_1_t5A6730315AA46B39985DF97B343C4E4ACEF77B42_il2cpp_TypeInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	{
		VisualEffect_t8CF4F0E13AEBF27224BB1323CC103666EDBBCFB0* L_0;
		L_0 = Component_GetComponent_TisVisualEffect_t8CF4F0E13AEBF27224BB1323CC103666EDBBCFB0_mC2DD444DE804211C8EADF53E224C7103F9A3CCD1(__this, Component_GetComponent_TisVisualEffect_t8CF4F0E13AEBF27224BB1323CC103666EDBBCFB0_mC2DD444DE804211C8EADF53E224C7103F9A3CCD1_RuntimeMethod_var);
		__this->___m_VisualEffect = L_0;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___m_VisualEffect), (void*)L_0);
		List_1_t5A6730315AA46B39985DF97B343C4E4ACEF77B42* L_1 = (List_1_t5A6730315AA46B39985DF97B343C4E4ACEF77B42*)il2cpp_codegen_object_new(List_1_t5A6730315AA46B39985DF97B343C4E4ACEF77B42_il2cpp_TypeInfo_var);
		List_1__ctor_m18A0E45DF3874B841F030036ABD1CBDCF7D77555(L_1, List_1__ctor_m18A0E45DF3874B841F030036ABD1CBDCF7D77555_RuntimeMethod_var);
		__this->___m_Bindings = L_1;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___m_Bindings), (void*)L_1);
		List_1_t5A6730315AA46B39985DF97B343C4E4ACEF77B42* L_2 = __this->___m_Bindings;
		GameObject_t76FEDD663AB33C991A9C9A23129337651094216F* L_3;
		L_3 = Component_get_gameObject_m57AEFBB14DB39EC476F740BA000E170355DE691B(__this, NULL);
		NullCheck(L_3);
		VFXBinderBaseU5BU5D_t1ECF22FF6432C40931AF7B0DEC149B606A33E247* L_4;
		L_4 = GameObject_GetComponents_TisVFXBinderBase_t8BC197F500B3A144DF76E77C48FA239C04CDA19C_m2849BF084DFC48ABA7745E3C7EC2249159F03323(L_3, GameObject_GetComponents_TisVFXBinderBase_t8BC197F500B3A144DF76E77C48FA239C04CDA19C_m2849BF084DFC48ABA7745E3C7EC2249159F03323_RuntimeMethod_var);
		NullCheck(L_2);
		List_1_AddRange_mD0480735EA168951277C2C2EB3F55E76699E8795(L_2, (RuntimeObject*)L_4, List_1_AddRange_mD0480735EA168951277C2C2EB3F55E76699E8795_RuntimeMethod_var);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void VFXPropertyBinder_Reset_mEE67C9046525E83EB402D542891D80149008D9D2 (VFXPropertyBinder_t761F7FEC5308F4D9ABF7F0A5A3FAAAA6FE043C0F* __this, const RuntimeMethod* method) 
{
	{
		VFXPropertyBinder_Reload_mC20E8C114F88956652BCFE1D1AB7701BA946CA4A(__this, NULL);
		VFXPropertyBinder_ClearPropertyBinders_m0D6B59D73389A5A83E46E87D05B24ADE479CF13F(__this, NULL);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void VFXPropertyBinder_LateUpdate_mBF71A331AE3C80F8CFE288BBCF1CBF7B20A505BC (VFXPropertyBinder_t761F7FEC5308F4D9ABF7F0A5A3FAAAA6FE043C0F* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Application_tDB03BE91CDF0ACA614A5E0B67CFB77C44EB19B21_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Debug_t8394C7EEAECA3689C2C9B9DE9C7166D73596276F_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&List_1_get_Count_mF8853CB1C215FFD0674D1337D23514D811BBF2B2_RuntimeMethod_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&List_1_get_Item_m33073C80B80A639ECF4F79B6BDBE709573170A4E_RuntimeMethod_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteral68E5CEC31382AB192FF06E62F708EC196A878699);
		s_Il2CppMethodInitialized = true;
	}
	int32_t V_0 = 0;
	VFXBinderBase_t8BC197F500B3A144DF76E77C48FA239C04CDA19C* V_1 = NULL;
	{
		bool L_0 = __this->___m_ExecuteInEditor;
		if (L_0)
		{
			goto IL_0017;
		}
	}
	{
		il2cpp_codegen_runtime_class_init_inline(Application_tDB03BE91CDF0ACA614A5E0B67CFB77C44EB19B21_il2cpp_TypeInfo_var);
		bool L_1;
		L_1 = Application_get_isEditor_mEAC51E3ACE6DCE438087FB14BD75A3C219D354D0(NULL);
		if (!L_1)
		{
			goto IL_0017;
		}
	}
	{
		il2cpp_codegen_runtime_class_init_inline(Application_tDB03BE91CDF0ACA614A5E0B67CFB77C44EB19B21_il2cpp_TypeInfo_var);
		bool L_2;
		L_2 = Application_get_isPlaying_m25B0ABDFEF54F5370CD3F263A813540843D00F34(NULL);
		if (L_2)
		{
			goto IL_0017;
		}
	}
	{
		return;
	}

IL_0017:
	{
		V_0 = 0;
		goto IL_0071;
	}

IL_001b:
	{
		List_1_t5A6730315AA46B39985DF97B343C4E4ACEF77B42* L_3 = __this->___m_Bindings;
		int32_t L_4 = V_0;
		NullCheck(L_3);
		VFXBinderBase_t8BC197F500B3A144DF76E77C48FA239C04CDA19C* L_5;
		L_5 = List_1_get_Item_m33073C80B80A639ECF4F79B6BDBE709573170A4E(L_3, L_4, List_1_get_Item_m33073C80B80A639ECF4F79B6BDBE709573170A4E_RuntimeMethod_var);
		V_1 = L_5;
		VFXBinderBase_t8BC197F500B3A144DF76E77C48FA239C04CDA19C* L_6 = V_1;
		il2cpp_codegen_runtime_class_init_inline(Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C_il2cpp_TypeInfo_var);
		bool L_7;
		L_7 = Object_op_Equality_mB6120F782D83091EF56A198FCEBCF066DB4A9605(L_6, (Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C*)NULL, NULL);
		if (!L_7)
		{
			goto IL_0053;
		}
	}
	{
		int32_t L_8 = V_0;
		int32_t L_9 = L_8;
		RuntimeObject* L_10 = Box(il2cpp_defaults.int32_class, &L_9);
		GameObject_t76FEDD663AB33C991A9C9A23129337651094216F* L_11;
		L_11 = Component_get_gameObject_m57AEFBB14DB39EC476F740BA000E170355DE691B(__this, NULL);
		NullCheck(L_11);
		String_t* L_12;
		L_12 = Object_get_name_mAC2F6B897CF1303BA4249B4CB55271AFACBB6392(L_11, NULL);
		String_t* L_13;
		L_13 = String_Format_mFB7DA489BD99F4670881FF50EC017BFB0A5C0987(_stringLiteral68E5CEC31382AB192FF06E62F708EC196A878699, L_10, L_12, NULL);
		il2cpp_codegen_runtime_class_init_inline(Debug_t8394C7EEAECA3689C2C9B9DE9C7166D73596276F_il2cpp_TypeInfo_var);
		Debug_LogWarning_m33EF1B897E0C7C6FF538989610BFAFFEF4628CA9(L_13, NULL);
		goto IL_006d;
	}

IL_0053:
	{
		VFXBinderBase_t8BC197F500B3A144DF76E77C48FA239C04CDA19C* L_14 = V_1;
		VisualEffect_t8CF4F0E13AEBF27224BB1323CC103666EDBBCFB0* L_15 = __this->___m_VisualEffect;
		NullCheck(L_14);
		bool L_16;
		L_16 = VirtualFuncInvoker1< bool, VisualEffect_t8CF4F0E13AEBF27224BB1323CC103666EDBBCFB0* >::Invoke(4, L_14, L_15);
		if (!L_16)
		{
			goto IL_006d;
		}
	}
	{
		VFXBinderBase_t8BC197F500B3A144DF76E77C48FA239C04CDA19C* L_17 = V_1;
		VisualEffect_t8CF4F0E13AEBF27224BB1323CC103666EDBBCFB0* L_18 = __this->___m_VisualEffect;
		NullCheck(L_17);
		VirtualActionInvoker1< VisualEffect_t8CF4F0E13AEBF27224BB1323CC103666EDBBCFB0* >::Invoke(9, L_17, L_18);
	}

IL_006d:
	{
		int32_t L_19 = V_0;
		V_0 = ((int32_t)il2cpp_codegen_add(L_19, 1));
	}

IL_0071:
	{
		int32_t L_20 = V_0;
		List_1_t5A6730315AA46B39985DF97B343C4E4ACEF77B42* L_21 = __this->___m_Bindings;
		NullCheck(L_21);
		int32_t L_22;
		L_22 = List_1_get_Count_mF8853CB1C215FFD0674D1337D23514D811BBF2B2_inline(L_21, List_1_get_Count_mF8853CB1C215FFD0674D1337D23514D811BBF2B2_RuntimeMethod_var);
		if ((((int32_t)L_20) < ((int32_t)L_22)))
		{
			goto IL_001b;
		}
	}
	{
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void VFXPropertyBinder_ClearPropertyBinders_m0D6B59D73389A5A83E46E87D05B24ADE479CF13F (VFXPropertyBinder_t761F7FEC5308F4D9ABF7F0A5A3FAAAA6FE043C0F* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Component_GetComponents_TisVFXBinderBase_t8BC197F500B3A144DF76E77C48FA239C04CDA19C_m53E17DF23C2BBA4AD8DE56C0C006D00288F75A05_RuntimeMethod_var);
		s_Il2CppMethodInitialized = true;
	}
	VFXBinderBaseU5BU5D_t1ECF22FF6432C40931AF7B0DEC149B606A33E247* V_0 = NULL;
	int32_t V_1 = 0;
	{
		VFXBinderBaseU5BU5D_t1ECF22FF6432C40931AF7B0DEC149B606A33E247* L_0;
		L_0 = Component_GetComponents_TisVFXBinderBase_t8BC197F500B3A144DF76E77C48FA239C04CDA19C_m53E17DF23C2BBA4AD8DE56C0C006D00288F75A05(__this, Component_GetComponents_TisVFXBinderBase_t8BC197F500B3A144DF76E77C48FA239C04CDA19C_m53E17DF23C2BBA4AD8DE56C0C006D00288F75A05_RuntimeMethod_var);
		V_0 = L_0;
		V_1 = 0;
		goto IL_0017;
	}

IL_000b:
	{
		VFXBinderBaseU5BU5D_t1ECF22FF6432C40931AF7B0DEC149B606A33E247* L_1 = V_0;
		int32_t L_2 = V_1;
		NullCheck(L_1);
		int32_t L_3 = L_2;
		VFXBinderBase_t8BC197F500B3A144DF76E77C48FA239C04CDA19C* L_4 = (L_1)->GetAt(static_cast<il2cpp_array_size_t>(L_3));
		VFXPropertyBinder_SafeDestroy_m4BC4B6A481989E373832AB7449F231ACB640F1AA(L_4, NULL);
		int32_t L_5 = V_1;
		V_1 = ((int32_t)il2cpp_codegen_add(L_5, 1));
	}

IL_0017:
	{
		int32_t L_6 = V_1;
		VFXBinderBaseU5BU5D_t1ECF22FF6432C40931AF7B0DEC149B606A33E247* L_7 = V_0;
		NullCheck(L_7);
		if ((((int32_t)L_6) < ((int32_t)((int32_t)(((RuntimeArray*)L_7)->max_length)))))
		{
			goto IL_000b;
		}
	}
	{
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void VFXPropertyBinder_ClearParameterBinders_mFD3FCF79B5B335810B8A754D503702EF15A06F6D (VFXPropertyBinder_t761F7FEC5308F4D9ABF7F0A5A3FAAAA6FE043C0F* __this, const RuntimeMethod* method) 
{
	{
		VFXPropertyBinder_ClearPropertyBinders_m0D6B59D73389A5A83E46E87D05B24ADE479CF13F(__this, NULL);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void VFXPropertyBinder_RemovePropertyBinder_mBFC783DFBF28ED3BB9ACC607CECCF9EF4661E863 (VFXPropertyBinder_t761F7FEC5308F4D9ABF7F0A5A3FAAAA6FE043C0F* __this, VFXBinderBase_t8BC197F500B3A144DF76E77C48FA239C04CDA19C* ___0_binder, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C_il2cpp_TypeInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	{
		VFXBinderBase_t8BC197F500B3A144DF76E77C48FA239C04CDA19C* L_0 = ___0_binder;
		NullCheck(L_0);
		GameObject_t76FEDD663AB33C991A9C9A23129337651094216F* L_1;
		L_1 = Component_get_gameObject_m57AEFBB14DB39EC476F740BA000E170355DE691B(L_0, NULL);
		GameObject_t76FEDD663AB33C991A9C9A23129337651094216F* L_2;
		L_2 = Component_get_gameObject_m57AEFBB14DB39EC476F740BA000E170355DE691B(__this, NULL);
		il2cpp_codegen_runtime_class_init_inline(Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C_il2cpp_TypeInfo_var);
		bool L_3;
		L_3 = Object_op_Equality_mB6120F782D83091EF56A198FCEBCF066DB4A9605(L_1, L_2, NULL);
		if (!L_3)
		{
			goto IL_0019;
		}
	}
	{
		VFXBinderBase_t8BC197F500B3A144DF76E77C48FA239C04CDA19C* L_4 = ___0_binder;
		VFXPropertyBinder_SafeDestroy_m4BC4B6A481989E373832AB7449F231ACB640F1AA(L_4, NULL);
	}

IL_0019:
	{
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void VFXPropertyBinder_RemoveParameterBinder_mCD45A1A2731CCE0B9B4946F22C21794719E534D6 (VFXPropertyBinder_t761F7FEC5308F4D9ABF7F0A5A3FAAAA6FE043C0F* __this, VFXBinderBase_t8BC197F500B3A144DF76E77C48FA239C04CDA19C* ___0_binder, const RuntimeMethod* method) 
{
	{
		VFXBinderBase_t8BC197F500B3A144DF76E77C48FA239C04CDA19C* L_0 = ___0_binder;
		VFXPropertyBinder_RemovePropertyBinder_mBFC783DFBF28ED3BB9ACC607CECCF9EF4661E863(__this, L_0, NULL);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void VFXPropertyBinder__ctor_mAC0E583E26FEA725E8C676DD293C791173AA8A90 (VFXPropertyBinder_t761F7FEC5308F4D9ABF7F0A5A3FAAAA6FE043C0F* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&List_1__ctor_m18A0E45DF3874B841F030036ABD1CBDCF7D77555_RuntimeMethod_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&List_1_t5A6730315AA46B39985DF97B343C4E4ACEF77B42_il2cpp_TypeInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	{
		__this->___m_ExecuteInEditor = (bool)1;
		List_1_t5A6730315AA46B39985DF97B343C4E4ACEF77B42* L_0 = (List_1_t5A6730315AA46B39985DF97B343C4E4ACEF77B42*)il2cpp_codegen_object_new(List_1_t5A6730315AA46B39985DF97B343C4E4ACEF77B42_il2cpp_TypeInfo_var);
		List_1__ctor_m18A0E45DF3874B841F030036ABD1CBDCF7D77555(L_0, List_1__ctor_m18A0E45DF3874B841F030036ABD1CBDCF7D77555_RuntimeMethod_var);
		__this->___m_Bindings = L_0;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___m_Bindings), (void*)L_0);
		MonoBehaviour__ctor_m592DB0105CA0BC97AA1C5F4AD27B12D68A3B7C1E(__this, NULL);
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
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void VFXPropertyBindingAttribute__ctor_m3619150CC621FD0D074BD7482574633A3F64ACB9 (VFXPropertyBindingAttribute_t600577C584AF5B4A36738E462D551C25B663311E* __this, StringU5BU5D_t7674CD946EC0CE7B3AE0BE70E6EE85F2ECD9F248* ___0_editorTypes, const RuntimeMethod* method) 
{
	{
		PropertyAttribute__ctor_m19247686E165101F140615C7306DC2DA3953D97D(__this, NULL);
		StringU5BU5D_t7674CD946EC0CE7B3AE0BE70E6EE85F2ECD9F248* L_0 = ___0_editorTypes;
		__this->___EditorTypes = L_0;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___EditorTypes), (void*)L_0);
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
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR String_t* ExposedProperty_op_Explicit_m8F7C257466E30C5602C4F12AEA2588B7093424F3_inline (ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* ___0_parameter, const RuntimeMethod* method) 
{
	{
		ExposedProperty_t90982415ECDCB651D6A637FE3DA1D53BC76D602A* L_0 = ___0_parameter;
		NullCheck(L_0);
		String_t* L_1 = L_0->___m_Name;
		return L_1;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR Vector4_t58B63D32F48C0DBF50DE2C60794C4676C80EDBE3 Color_op_Implicit_m9B3228DAFA8DC57A75DE00CBBF13ED4F1E7B01FF_inline (Color_tD001788D726C3A7F1379BEED0260B9591F440C1F ___0_c, const RuntimeMethod* method) 
{
	Vector4_t58B63D32F48C0DBF50DE2C60794C4676C80EDBE3 V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		il2cpp_codegen_initobj((&V_0), sizeof(Vector4_t58B63D32F48C0DBF50DE2C60794C4676C80EDBE3));
		Color_tD001788D726C3A7F1379BEED0260B9591F440C1F L_0 = ___0_c;
		float L_1 = L_0.___r;
		(&V_0)->___x = L_1;
		Color_tD001788D726C3A7F1379BEED0260B9591F440C1F L_2 = ___0_c;
		float L_3 = L_2.___g;
		(&V_0)->___y = L_3;
		Color_tD001788D726C3A7F1379BEED0260B9591F440C1F L_4 = ___0_c;
		float L_5 = L_4.___b;
		(&V_0)->___z = L_5;
		Color_tD001788D726C3A7F1379BEED0260B9591F440C1F L_6 = ___0_c;
		float L_7 = L_6.___a;
		(&V_0)->___w = L_7;
		Vector4_t58B63D32F48C0DBF50DE2C60794C4676C80EDBE3 L_8 = V_0;
		return L_8;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void Color__ctor_mCD6889CDE39F18704CD6EA8E2EFBFA48BA3E13B0_inline (Color_tD001788D726C3A7F1379BEED0260B9591F440C1F* __this, float ___0_r, float ___1_g, float ___2_b, const RuntimeMethod* method) 
{
	{
		float L_0 = ___0_r;
		__this->___r = L_0;
		float L_1 = ___1_g;
		__this->___g = L_1;
		float L_2 = ___2_b;
		__this->___b = L_2;
		__this->___a = (1.0f);
		return;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 Vector3_get_zero_m0C1249C3F25B1C70EAD3CC8B31259975A457AE39_inline (const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2_il2cpp_TypeInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	{
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_0 = ((Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2_StaticFields*)il2cpp_codegen_static_fields_for(Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2_il2cpp_TypeInfo_var))->___zeroVector;
		return L_0;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void Ray__ctor_mE298992FD10A3894C38373198385F345C58BD64C_inline (Ray_t2B1742D7958DC05BDC3EFC7461D3593E1430DC00* __this, Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 ___0_origin, Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 ___1_direction, const RuntimeMethod* method) 
{
	{
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_0 = ___0_origin;
		__this->___m_Origin = L_0;
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_1 = ___1_direction;
		__this->___m_Direction = L_1;
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2* L_2 = (Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2*)(&__this->___m_Direction);
		Vector3_Normalize_mC749B887A4C74BA0A2E13E6377F17CCAEB0AADA8_inline(L_2, NULL);
		return;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 Vector3_get_forward_mAA55A7034304DF8B2152EAD49AE779FC4CA2EB4A_inline (const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2_il2cpp_TypeInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	{
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_0 = ((Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2_StaticFields*)il2cpp_codegen_static_fields_for(Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2_il2cpp_TypeInfo_var))->___forwardVector;
		return L_0;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 Matrix4x4_op_Multiply_m75E91775655DCA8DFC8EDE0AB787285BB3935162_inline (Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 ___0_lhs, Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 ___1_rhs, const RuntimeMethod* method) 
{
	Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		il2cpp_codegen_initobj((&V_0), sizeof(Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6));
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_0 = ___0_lhs;
		float L_1 = L_0.___m00;
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_2 = ___1_rhs;
		float L_3 = L_2.___m00;
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_4 = ___0_lhs;
		float L_5 = L_4.___m01;
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_6 = ___1_rhs;
		float L_7 = L_6.___m10;
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_8 = ___0_lhs;
		float L_9 = L_8.___m02;
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_10 = ___1_rhs;
		float L_11 = L_10.___m20;
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_12 = ___0_lhs;
		float L_13 = L_12.___m03;
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_14 = ___1_rhs;
		float L_15 = L_14.___m30;
		(&V_0)->___m00 = ((float)il2cpp_codegen_add(((float)il2cpp_codegen_add(((float)il2cpp_codegen_add(((float)il2cpp_codegen_multiply(L_1, L_3)), ((float)il2cpp_codegen_multiply(L_5, L_7)))), ((float)il2cpp_codegen_multiply(L_9, L_11)))), ((float)il2cpp_codegen_multiply(L_13, L_15))));
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_16 = ___0_lhs;
		float L_17 = L_16.___m00;
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_18 = ___1_rhs;
		float L_19 = L_18.___m01;
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_20 = ___0_lhs;
		float L_21 = L_20.___m01;
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_22 = ___1_rhs;
		float L_23 = L_22.___m11;
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_24 = ___0_lhs;
		float L_25 = L_24.___m02;
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_26 = ___1_rhs;
		float L_27 = L_26.___m21;
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_28 = ___0_lhs;
		float L_29 = L_28.___m03;
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_30 = ___1_rhs;
		float L_31 = L_30.___m31;
		(&V_0)->___m01 = ((float)il2cpp_codegen_add(((float)il2cpp_codegen_add(((float)il2cpp_codegen_add(((float)il2cpp_codegen_multiply(L_17, L_19)), ((float)il2cpp_codegen_multiply(L_21, L_23)))), ((float)il2cpp_codegen_multiply(L_25, L_27)))), ((float)il2cpp_codegen_multiply(L_29, L_31))));
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_32 = ___0_lhs;
		float L_33 = L_32.___m00;
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_34 = ___1_rhs;
		float L_35 = L_34.___m02;
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_36 = ___0_lhs;
		float L_37 = L_36.___m01;
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_38 = ___1_rhs;
		float L_39 = L_38.___m12;
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_40 = ___0_lhs;
		float L_41 = L_40.___m02;
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_42 = ___1_rhs;
		float L_43 = L_42.___m22;
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_44 = ___0_lhs;
		float L_45 = L_44.___m03;
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_46 = ___1_rhs;
		float L_47 = L_46.___m32;
		(&V_0)->___m02 = ((float)il2cpp_codegen_add(((float)il2cpp_codegen_add(((float)il2cpp_codegen_add(((float)il2cpp_codegen_multiply(L_33, L_35)), ((float)il2cpp_codegen_multiply(L_37, L_39)))), ((float)il2cpp_codegen_multiply(L_41, L_43)))), ((float)il2cpp_codegen_multiply(L_45, L_47))));
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_48 = ___0_lhs;
		float L_49 = L_48.___m00;
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_50 = ___1_rhs;
		float L_51 = L_50.___m03;
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_52 = ___0_lhs;
		float L_53 = L_52.___m01;
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_54 = ___1_rhs;
		float L_55 = L_54.___m13;
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_56 = ___0_lhs;
		float L_57 = L_56.___m02;
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_58 = ___1_rhs;
		float L_59 = L_58.___m23;
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_60 = ___0_lhs;
		float L_61 = L_60.___m03;
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_62 = ___1_rhs;
		float L_63 = L_62.___m33;
		(&V_0)->___m03 = ((float)il2cpp_codegen_add(((float)il2cpp_codegen_add(((float)il2cpp_codegen_add(((float)il2cpp_codegen_multiply(L_49, L_51)), ((float)il2cpp_codegen_multiply(L_53, L_55)))), ((float)il2cpp_codegen_multiply(L_57, L_59)))), ((float)il2cpp_codegen_multiply(L_61, L_63))));
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_64 = ___0_lhs;
		float L_65 = L_64.___m10;
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_66 = ___1_rhs;
		float L_67 = L_66.___m00;
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_68 = ___0_lhs;
		float L_69 = L_68.___m11;
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_70 = ___1_rhs;
		float L_71 = L_70.___m10;
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_72 = ___0_lhs;
		float L_73 = L_72.___m12;
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_74 = ___1_rhs;
		float L_75 = L_74.___m20;
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_76 = ___0_lhs;
		float L_77 = L_76.___m13;
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_78 = ___1_rhs;
		float L_79 = L_78.___m30;
		(&V_0)->___m10 = ((float)il2cpp_codegen_add(((float)il2cpp_codegen_add(((float)il2cpp_codegen_add(((float)il2cpp_codegen_multiply(L_65, L_67)), ((float)il2cpp_codegen_multiply(L_69, L_71)))), ((float)il2cpp_codegen_multiply(L_73, L_75)))), ((float)il2cpp_codegen_multiply(L_77, L_79))));
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_80 = ___0_lhs;
		float L_81 = L_80.___m10;
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_82 = ___1_rhs;
		float L_83 = L_82.___m01;
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_84 = ___0_lhs;
		float L_85 = L_84.___m11;
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_86 = ___1_rhs;
		float L_87 = L_86.___m11;
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_88 = ___0_lhs;
		float L_89 = L_88.___m12;
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_90 = ___1_rhs;
		float L_91 = L_90.___m21;
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_92 = ___0_lhs;
		float L_93 = L_92.___m13;
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_94 = ___1_rhs;
		float L_95 = L_94.___m31;
		(&V_0)->___m11 = ((float)il2cpp_codegen_add(((float)il2cpp_codegen_add(((float)il2cpp_codegen_add(((float)il2cpp_codegen_multiply(L_81, L_83)), ((float)il2cpp_codegen_multiply(L_85, L_87)))), ((float)il2cpp_codegen_multiply(L_89, L_91)))), ((float)il2cpp_codegen_multiply(L_93, L_95))));
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_96 = ___0_lhs;
		float L_97 = L_96.___m10;
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_98 = ___1_rhs;
		float L_99 = L_98.___m02;
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_100 = ___0_lhs;
		float L_101 = L_100.___m11;
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_102 = ___1_rhs;
		float L_103 = L_102.___m12;
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_104 = ___0_lhs;
		float L_105 = L_104.___m12;
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_106 = ___1_rhs;
		float L_107 = L_106.___m22;
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_108 = ___0_lhs;
		float L_109 = L_108.___m13;
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_110 = ___1_rhs;
		float L_111 = L_110.___m32;
		(&V_0)->___m12 = ((float)il2cpp_codegen_add(((float)il2cpp_codegen_add(((float)il2cpp_codegen_add(((float)il2cpp_codegen_multiply(L_97, L_99)), ((float)il2cpp_codegen_multiply(L_101, L_103)))), ((float)il2cpp_codegen_multiply(L_105, L_107)))), ((float)il2cpp_codegen_multiply(L_109, L_111))));
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_112 = ___0_lhs;
		float L_113 = L_112.___m10;
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_114 = ___1_rhs;
		float L_115 = L_114.___m03;
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_116 = ___0_lhs;
		float L_117 = L_116.___m11;
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_118 = ___1_rhs;
		float L_119 = L_118.___m13;
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_120 = ___0_lhs;
		float L_121 = L_120.___m12;
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_122 = ___1_rhs;
		float L_123 = L_122.___m23;
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_124 = ___0_lhs;
		float L_125 = L_124.___m13;
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_126 = ___1_rhs;
		float L_127 = L_126.___m33;
		(&V_0)->___m13 = ((float)il2cpp_codegen_add(((float)il2cpp_codegen_add(((float)il2cpp_codegen_add(((float)il2cpp_codegen_multiply(L_113, L_115)), ((float)il2cpp_codegen_multiply(L_117, L_119)))), ((float)il2cpp_codegen_multiply(L_121, L_123)))), ((float)il2cpp_codegen_multiply(L_125, L_127))));
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_128 = ___0_lhs;
		float L_129 = L_128.___m20;
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_130 = ___1_rhs;
		float L_131 = L_130.___m00;
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_132 = ___0_lhs;
		float L_133 = L_132.___m21;
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_134 = ___1_rhs;
		float L_135 = L_134.___m10;
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_136 = ___0_lhs;
		float L_137 = L_136.___m22;
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_138 = ___1_rhs;
		float L_139 = L_138.___m20;
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_140 = ___0_lhs;
		float L_141 = L_140.___m23;
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_142 = ___1_rhs;
		float L_143 = L_142.___m30;
		(&V_0)->___m20 = ((float)il2cpp_codegen_add(((float)il2cpp_codegen_add(((float)il2cpp_codegen_add(((float)il2cpp_codegen_multiply(L_129, L_131)), ((float)il2cpp_codegen_multiply(L_133, L_135)))), ((float)il2cpp_codegen_multiply(L_137, L_139)))), ((float)il2cpp_codegen_multiply(L_141, L_143))));
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_144 = ___0_lhs;
		float L_145 = L_144.___m20;
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_146 = ___1_rhs;
		float L_147 = L_146.___m01;
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_148 = ___0_lhs;
		float L_149 = L_148.___m21;
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_150 = ___1_rhs;
		float L_151 = L_150.___m11;
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_152 = ___0_lhs;
		float L_153 = L_152.___m22;
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_154 = ___1_rhs;
		float L_155 = L_154.___m21;
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_156 = ___0_lhs;
		float L_157 = L_156.___m23;
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_158 = ___1_rhs;
		float L_159 = L_158.___m31;
		(&V_0)->___m21 = ((float)il2cpp_codegen_add(((float)il2cpp_codegen_add(((float)il2cpp_codegen_add(((float)il2cpp_codegen_multiply(L_145, L_147)), ((float)il2cpp_codegen_multiply(L_149, L_151)))), ((float)il2cpp_codegen_multiply(L_153, L_155)))), ((float)il2cpp_codegen_multiply(L_157, L_159))));
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_160 = ___0_lhs;
		float L_161 = L_160.___m20;
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_162 = ___1_rhs;
		float L_163 = L_162.___m02;
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_164 = ___0_lhs;
		float L_165 = L_164.___m21;
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_166 = ___1_rhs;
		float L_167 = L_166.___m12;
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_168 = ___0_lhs;
		float L_169 = L_168.___m22;
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_170 = ___1_rhs;
		float L_171 = L_170.___m22;
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_172 = ___0_lhs;
		float L_173 = L_172.___m23;
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_174 = ___1_rhs;
		float L_175 = L_174.___m32;
		(&V_0)->___m22 = ((float)il2cpp_codegen_add(((float)il2cpp_codegen_add(((float)il2cpp_codegen_add(((float)il2cpp_codegen_multiply(L_161, L_163)), ((float)il2cpp_codegen_multiply(L_165, L_167)))), ((float)il2cpp_codegen_multiply(L_169, L_171)))), ((float)il2cpp_codegen_multiply(L_173, L_175))));
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_176 = ___0_lhs;
		float L_177 = L_176.___m20;
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_178 = ___1_rhs;
		float L_179 = L_178.___m03;
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_180 = ___0_lhs;
		float L_181 = L_180.___m21;
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_182 = ___1_rhs;
		float L_183 = L_182.___m13;
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_184 = ___0_lhs;
		float L_185 = L_184.___m22;
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_186 = ___1_rhs;
		float L_187 = L_186.___m23;
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_188 = ___0_lhs;
		float L_189 = L_188.___m23;
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_190 = ___1_rhs;
		float L_191 = L_190.___m33;
		(&V_0)->___m23 = ((float)il2cpp_codegen_add(((float)il2cpp_codegen_add(((float)il2cpp_codegen_add(((float)il2cpp_codegen_multiply(L_177, L_179)), ((float)il2cpp_codegen_multiply(L_181, L_183)))), ((float)il2cpp_codegen_multiply(L_185, L_187)))), ((float)il2cpp_codegen_multiply(L_189, L_191))));
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_192 = ___0_lhs;
		float L_193 = L_192.___m30;
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_194 = ___1_rhs;
		float L_195 = L_194.___m00;
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_196 = ___0_lhs;
		float L_197 = L_196.___m31;
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_198 = ___1_rhs;
		float L_199 = L_198.___m10;
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_200 = ___0_lhs;
		float L_201 = L_200.___m32;
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_202 = ___1_rhs;
		float L_203 = L_202.___m20;
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_204 = ___0_lhs;
		float L_205 = L_204.___m33;
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_206 = ___1_rhs;
		float L_207 = L_206.___m30;
		(&V_0)->___m30 = ((float)il2cpp_codegen_add(((float)il2cpp_codegen_add(((float)il2cpp_codegen_add(((float)il2cpp_codegen_multiply(L_193, L_195)), ((float)il2cpp_codegen_multiply(L_197, L_199)))), ((float)il2cpp_codegen_multiply(L_201, L_203)))), ((float)il2cpp_codegen_multiply(L_205, L_207))));
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_208 = ___0_lhs;
		float L_209 = L_208.___m30;
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_210 = ___1_rhs;
		float L_211 = L_210.___m01;
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_212 = ___0_lhs;
		float L_213 = L_212.___m31;
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_214 = ___1_rhs;
		float L_215 = L_214.___m11;
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_216 = ___0_lhs;
		float L_217 = L_216.___m32;
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_218 = ___1_rhs;
		float L_219 = L_218.___m21;
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_220 = ___0_lhs;
		float L_221 = L_220.___m33;
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_222 = ___1_rhs;
		float L_223 = L_222.___m31;
		(&V_0)->___m31 = ((float)il2cpp_codegen_add(((float)il2cpp_codegen_add(((float)il2cpp_codegen_add(((float)il2cpp_codegen_multiply(L_209, L_211)), ((float)il2cpp_codegen_multiply(L_213, L_215)))), ((float)il2cpp_codegen_multiply(L_217, L_219)))), ((float)il2cpp_codegen_multiply(L_221, L_223))));
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_224 = ___0_lhs;
		float L_225 = L_224.___m30;
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_226 = ___1_rhs;
		float L_227 = L_226.___m02;
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_228 = ___0_lhs;
		float L_229 = L_228.___m31;
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_230 = ___1_rhs;
		float L_231 = L_230.___m12;
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_232 = ___0_lhs;
		float L_233 = L_232.___m32;
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_234 = ___1_rhs;
		float L_235 = L_234.___m22;
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_236 = ___0_lhs;
		float L_237 = L_236.___m33;
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_238 = ___1_rhs;
		float L_239 = L_238.___m32;
		(&V_0)->___m32 = ((float)il2cpp_codegen_add(((float)il2cpp_codegen_add(((float)il2cpp_codegen_add(((float)il2cpp_codegen_multiply(L_225, L_227)), ((float)il2cpp_codegen_multiply(L_229, L_231)))), ((float)il2cpp_codegen_multiply(L_233, L_235)))), ((float)il2cpp_codegen_multiply(L_237, L_239))));
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_240 = ___0_lhs;
		float L_241 = L_240.___m30;
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_242 = ___1_rhs;
		float L_243 = L_242.___m03;
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_244 = ___0_lhs;
		float L_245 = L_244.___m31;
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_246 = ___1_rhs;
		float L_247 = L_246.___m13;
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_248 = ___0_lhs;
		float L_249 = L_248.___m32;
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_250 = ___1_rhs;
		float L_251 = L_250.___m23;
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_252 = ___0_lhs;
		float L_253 = L_252.___m33;
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_254 = ___1_rhs;
		float L_255 = L_254.___m33;
		(&V_0)->___m33 = ((float)il2cpp_codegen_add(((float)il2cpp_codegen_add(((float)il2cpp_codegen_add(((float)il2cpp_codegen_multiply(L_241, L_243)), ((float)il2cpp_codegen_multiply(L_245, L_247)))), ((float)il2cpp_codegen_multiply(L_249, L_251)))), ((float)il2cpp_codegen_multiply(L_253, L_255))));
		Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6 L_256 = V_0;
		return L_256;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 Matrix4x4_GetPosition_m9C2F34474967E47C83886A2EC335DFE37B563E80_inline (Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6* __this, const RuntimeMethod* method) 
{
	Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		il2cpp_codegen_initobj((&V_0), sizeof(Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2));
		float L_0 = __this->___m03;
		(&V_0)->___x = L_0;
		float L_1 = __this->___m13;
		(&V_0)->___y = L_1;
		float L_2 = __this->___m23;
		(&V_0)->___z = L_2;
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_3 = V_0;
		return L_3;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 Vector3_get_up_m128AF3FDC820BF59D5DE86D973E7DE3F20C3AEBA_inline (const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2_il2cpp_TypeInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	{
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_0 = ((Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2_StaticFields*)il2cpp_codegen_static_fields_for(Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2_il2cpp_TypeInfo_var))->___upVector;
		return L_0;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 Matrix4x4_MultiplyVector_mFD12F86A473E90BBB0002149ABA3917B2A518937_inline (Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6* __this, Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 ___0_vector, const RuntimeMethod* method) 
{
	Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		il2cpp_codegen_initobj((&V_0), sizeof(Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2));
		float L_0 = __this->___m00;
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_1 = ___0_vector;
		float L_2 = L_1.___x;
		float L_3 = __this->___m01;
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_4 = ___0_vector;
		float L_5 = L_4.___y;
		float L_6 = __this->___m02;
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_7 = ___0_vector;
		float L_8 = L_7.___z;
		(&V_0)->___x = ((float)il2cpp_codegen_add(((float)il2cpp_codegen_add(((float)il2cpp_codegen_multiply(L_0, L_2)), ((float)il2cpp_codegen_multiply(L_3, L_5)))), ((float)il2cpp_codegen_multiply(L_6, L_8))));
		float L_9 = __this->___m10;
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_10 = ___0_vector;
		float L_11 = L_10.___x;
		float L_12 = __this->___m11;
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_13 = ___0_vector;
		float L_14 = L_13.___y;
		float L_15 = __this->___m12;
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_16 = ___0_vector;
		float L_17 = L_16.___z;
		(&V_0)->___y = ((float)il2cpp_codegen_add(((float)il2cpp_codegen_add(((float)il2cpp_codegen_multiply(L_9, L_11)), ((float)il2cpp_codegen_multiply(L_12, L_14)))), ((float)il2cpp_codegen_multiply(L_15, L_17))));
		float L_18 = __this->___m20;
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_19 = ___0_vector;
		float L_20 = L_19.___x;
		float L_21 = __this->___m21;
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_22 = ___0_vector;
		float L_23 = L_22.___y;
		float L_24 = __this->___m22;
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_25 = ___0_vector;
		float L_26 = L_25.___z;
		(&V_0)->___z = ((float)il2cpp_codegen_add(((float)il2cpp_codegen_add(((float)il2cpp_codegen_multiply(L_18, L_20)), ((float)il2cpp_codegen_multiply(L_21, L_23)))), ((float)il2cpp_codegen_multiply(L_24, L_26))));
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_27 = V_0;
		return L_27;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 Matrix4x4_get_lossyScale_mFB3D4CF6408D710D607CA1D2AF94B2E4E0B57EB7_inline (Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6* __this, const RuntimeMethod* method) 
{
	{
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_0;
		L_0 = Matrix4x4_GetLossyScale_m3C19D2C6746BB211C8CB02478A60EB2D71D10FC7(__this, NULL);
		return L_0;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974 Matrix4x4_get_rotation_m7E2C29FCB2AAFAE4D7B4FBD3563E9EDB53F5A8BB_inline (Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6* __this, const RuntimeMethod* method) 
{
	{
		Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974 L_0;
		L_0 = Matrix4x4_GetRotation_mA3388AAB0DC4960D4E93D945398430A5C62D4D6F(__this, NULL);
		return L_0;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 Quaternion_get_eulerAngles_m2DB5158B5C3A71FD60FC8A6EE43D3AAA1CFED122_inline (Quaternion_tDA59F214EF07D7700B26E40E562F267AF7306974* __this, const RuntimeMethod* method) 
{
	{
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_0;
		L_0 = Quaternion_Internal_ToEulerRad_mC5BD020889B5A4FB6894CFB69A5D4C07B321B919(__this, NULL);
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_1;
		L_1 = Vector3_op_Multiply_m87BA7C578F96C8E49BB07088DAAC4649F83B0353_inline(L_0, (57.2957802f), NULL);
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_2;
		L_2 = Quaternion_Internal_MakePositive_m73E2D01920CB0DFE661A55022C129E8617F0C9A8(L_1, NULL);
		return L_2;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 Matrix4x4_MultiplyPoint_m20E910B65693559BFDE99382472D8DD02C862E7E_inline (Matrix4x4_tDB70CF134A14BA38190C59AA700BCE10E2AED3E6* __this, Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 ___0_point, const RuntimeMethod* method) 
{
	Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 V_0;
	memset((&V_0), 0, sizeof(V_0));
	float V_1 = 0.0f;
	Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 V_2;
	memset((&V_2), 0, sizeof(V_2));
	{
		float L_0 = __this->___m00;
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_1 = ___0_point;
		float L_2 = L_1.___x;
		float L_3 = __this->___m01;
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_4 = ___0_point;
		float L_5 = L_4.___y;
		float L_6 = __this->___m02;
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_7 = ___0_point;
		float L_8 = L_7.___z;
		float L_9 = __this->___m03;
		(&V_0)->___x = ((float)il2cpp_codegen_add(((float)il2cpp_codegen_add(((float)il2cpp_codegen_add(((float)il2cpp_codegen_multiply(L_0, L_2)), ((float)il2cpp_codegen_multiply(L_3, L_5)))), ((float)il2cpp_codegen_multiply(L_6, L_8)))), L_9));
		float L_10 = __this->___m10;
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_11 = ___0_point;
		float L_12 = L_11.___x;
		float L_13 = __this->___m11;
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_14 = ___0_point;
		float L_15 = L_14.___y;
		float L_16 = __this->___m12;
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_17 = ___0_point;
		float L_18 = L_17.___z;
		float L_19 = __this->___m13;
		(&V_0)->___y = ((float)il2cpp_codegen_add(((float)il2cpp_codegen_add(((float)il2cpp_codegen_add(((float)il2cpp_codegen_multiply(L_10, L_12)), ((float)il2cpp_codegen_multiply(L_13, L_15)))), ((float)il2cpp_codegen_multiply(L_16, L_18)))), L_19));
		float L_20 = __this->___m20;
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_21 = ___0_point;
		float L_22 = L_21.___x;
		float L_23 = __this->___m21;
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_24 = ___0_point;
		float L_25 = L_24.___y;
		float L_26 = __this->___m22;
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_27 = ___0_point;
		float L_28 = L_27.___z;
		float L_29 = __this->___m23;
		(&V_0)->___z = ((float)il2cpp_codegen_add(((float)il2cpp_codegen_add(((float)il2cpp_codegen_add(((float)il2cpp_codegen_multiply(L_20, L_22)), ((float)il2cpp_codegen_multiply(L_23, L_25)))), ((float)il2cpp_codegen_multiply(L_26, L_28)))), L_29));
		float L_30 = __this->___m30;
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_31 = ___0_point;
		float L_32 = L_31.___x;
		float L_33 = __this->___m31;
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_34 = ___0_point;
		float L_35 = L_34.___y;
		float L_36 = __this->___m32;
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_37 = ___0_point;
		float L_38 = L_37.___z;
		float L_39 = __this->___m33;
		V_1 = ((float)il2cpp_codegen_add(((float)il2cpp_codegen_add(((float)il2cpp_codegen_add(((float)il2cpp_codegen_multiply(L_30, L_32)), ((float)il2cpp_codegen_multiply(L_33, L_35)))), ((float)il2cpp_codegen_multiply(L_36, L_38)))), L_39));
		float L_40 = V_1;
		V_1 = ((float)((1.0f)/L_40));
		float* L_41 = (float*)(&(&V_0)->___x);
		float* L_42 = L_41;
		float L_43 = *((float*)L_42);
		float L_44 = V_1;
		*((float*)L_42) = (float)((float)il2cpp_codegen_multiply(L_43, L_44));
		float* L_45 = (float*)(&(&V_0)->___y);
		float* L_46 = L_45;
		float L_47 = *((float*)L_46);
		float L_48 = V_1;
		*((float*)L_46) = (float)((float)il2cpp_codegen_multiply(L_47, L_48));
		float* L_49 = (float*)(&(&V_0)->___z);
		float* L_50 = L_49;
		float L_51 = *((float*)L_50);
		float L_52 = V_1;
		*((float*)L_50) = (float)((float)il2cpp_codegen_multiply(L_51, L_52));
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_53 = V_0;
		V_2 = L_53;
		goto IL_0107;
	}

IL_0107:
	{
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_54 = V_2;
		return L_54;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 Vector3_op_Addition_m78C0EC70CB66E8DCAC225743D82B268DAEE92067_inline (Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 ___0_a, Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 ___1_b, const RuntimeMethod* method) 
{
	Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		il2cpp_codegen_initobj((&V_0), sizeof(Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2));
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_0 = ___0_a;
		float L_1 = L_0.___x;
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_2 = ___1_b;
		float L_3 = L_2.___x;
		(&V_0)->___x = ((float)il2cpp_codegen_add(L_1, L_3));
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_4 = ___0_a;
		float L_5 = L_4.___y;
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_6 = ___1_b;
		float L_7 = L_6.___y;
		(&V_0)->___y = ((float)il2cpp_codegen_add(L_5, L_7));
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_8 = ___0_a;
		float L_9 = L_8.___z;
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_10 = ___1_b;
		float L_11 = L_10.___z;
		(&V_0)->___z = ((float)il2cpp_codegen_add(L_9, L_11));
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_12 = V_0;
		return L_12;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR float Mathf_Max_mF5379E63D2BBAC76D090748695D833934F8AD051_inline (float ___0_a, float ___1_b, const RuntimeMethod* method) 
{
	float V_0 = 0.0f;
	float G_B3_0 = 0.0f;
	{
		float L_0 = ___0_a;
		float L_1 = ___1_b;
		if ((((float)L_0) > ((float)L_1)))
		{
			goto IL_0008;
		}
	}
	{
		float L_2 = ___1_b;
		G_B3_0 = L_2;
		goto IL_0009;
	}

IL_0008:
	{
		float L_3 = ___0_a;
		G_B3_0 = L_3;
	}

IL_0009:
	{
		V_0 = G_B3_0;
		goto IL_000c;
	}

IL_000c:
	{
		float L_4 = V_0;
		return L_4;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 Bounds_get_center_m5B05F81CB835EB6DD8628FDA24B638F477984DC3_inline (Bounds_t367E830C64BBF235ED8C3B2F8CF6254FDCAD39C3* __this, const RuntimeMethod* method) 
{
	{
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_0 = __this->___m_Center;
		return L_0;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 Bounds_get_size_m0699A53A55A78B3201D7270D6F338DFA91B6FAD4_inline (Bounds_t367E830C64BBF235ED8C3B2F8CF6254FDCAD39C3* __this, const RuntimeMethod* method) 
{
	Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		il2cpp_codegen_initobj((&V_0), sizeof(Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2));
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2* L_0 = (Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2*)(&__this->___m_Extents);
		float L_1 = L_0->___x;
		(&V_0)->___x = ((float)il2cpp_codegen_multiply(L_1, (2.0f)));
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2* L_2 = (Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2*)(&__this->___m_Extents);
		float L_3 = L_2->___y;
		(&V_0)->___y = ((float)il2cpp_codegen_multiply(L_3, (2.0f)));
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2* L_4 = (Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2*)(&__this->___m_Extents);
		float L_5 = L_4->___z;
		(&V_0)->___z = ((float)il2cpp_codegen_multiply(L_5, (2.0f)));
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_6 = V_0;
		return L_6;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR int32_t Dropdown_get_value_m386913162D5E273B762657FE5156DC567602BC3C_inline (Dropdown_t54C0BDC1441E058BE37E796F68886671C270EF89* __this, const RuntimeMethod* method) 
{
	{
		int32_t L_0 = __this->___m_Value;
		return L_0;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR bool Toggle_get_isOn_m89A609E936CD67F460E336CA8E03C4047BFB6619_inline (Toggle_tBF13F3EBA485E06826FD8A38F4B4C1380DF21A1F* __this, const RuntimeMethod* method) 
{
	{
		bool L_0 = __this->___m_IsOn;
		return L_0;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 Vector3_op_Subtraction_mE42023FF80067CB44A1D4A27EB7CF2B24CABB828_inline (Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 ___0_a, Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 ___1_b, const RuntimeMethod* method) 
{
	Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		il2cpp_codegen_initobj((&V_0), sizeof(Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2));
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_0 = ___0_a;
		float L_1 = L_0.___x;
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_2 = ___1_b;
		float L_3 = L_2.___x;
		(&V_0)->___x = ((float)il2cpp_codegen_subtract(L_1, L_3));
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_4 = ___0_a;
		float L_5 = L_4.___y;
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_6 = ___1_b;
		float L_7 = L_6.___y;
		(&V_0)->___y = ((float)il2cpp_codegen_subtract(L_5, L_7));
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_8 = ___0_a;
		float L_9 = L_8.___z;
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_10 = ___1_b;
		float L_11 = L_10.___z;
		(&V_0)->___z = ((float)il2cpp_codegen_subtract(L_9, L_11));
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_12 = V_0;
		return L_12;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR float Vector3_SqrMagnitude_m5ED73273031577902F1BED4CA9C5A0E786A680D1_inline (Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 ___0_vector, const RuntimeMethod* method) 
{
	{
		float L_0;
		L_0 = Vector3_get_sqrMagnitude_m43C27DEC47C4811FB30AB474FF2131A963B66FC8_inline((&___0_vector), NULL);
		return L_0;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 Vector3_op_Division_mCC6BB24E372AB96B8380D1678446EF6A8BAE13BB_inline (Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 ___0_a, float ___1_d, const RuntimeMethod* method) 
{
	Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		il2cpp_codegen_initobj((&V_0), sizeof(Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2));
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_0 = ___0_a;
		float L_1 = L_0.___x;
		float L_2 = ___1_d;
		(&V_0)->___x = ((float)(L_1/L_2));
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_3 = ___0_a;
		float L_4 = L_3.___y;
		float L_5 = ___1_d;
		(&V_0)->___y = ((float)(L_4/L_5));
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_6 = ___0_a;
		float L_7 = L_6.___z;
		float L_8 = ___1_d;
		(&V_0)->___z = ((float)(L_7/L_8));
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_9 = V_0;
		return L_9;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void List_1_Add_m79E50C4F592B1703F4B76A8BE7B4855515460CA1_gshared_inline (List_1_t77B94703E05C519A9010DD0614F757F974E1CD8B* __this, Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 ___0_item, const RuntimeMethod* method) 
{
	Vector3U5BU5D_tFF1859CCE176131B909E2044F76443064254679C* V_0 = NULL;
	int32_t V_1 = 0;
	{
		int32_t L_0 = __this->____version;
		__this->____version = ((int32_t)il2cpp_codegen_add(L_0, 1));
		Vector3U5BU5D_tFF1859CCE176131B909E2044F76443064254679C* L_1 = __this->____items;
		V_0 = L_1;
		int32_t L_2 = __this->____size;
		V_1 = L_2;
		int32_t L_3 = V_1;
		Vector3U5BU5D_tFF1859CCE176131B909E2044F76443064254679C* L_4 = V_0;
		NullCheck(L_4);
		if ((!(((uint32_t)L_3) < ((uint32_t)((int32_t)(((RuntimeArray*)L_4)->max_length))))))
		{
			goto IL_0034;
		}
	}
	{
		int32_t L_5 = V_1;
		__this->____size = ((int32_t)il2cpp_codegen_add(L_5, 1));
		Vector3U5BU5D_tFF1859CCE176131B909E2044F76443064254679C* L_6 = V_0;
		int32_t L_7 = V_1;
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_8 = ___0_item;
		NullCheck(L_6);
		(L_6)->SetAt(static_cast<il2cpp_array_size_t>(L_7), (Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2)L_8);
		return;
	}

IL_0034:
	{
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_9 = ___0_item;
		List_1_AddWithResize_mCDB189127FC2EE67B5EED9DE34F837DFC6EDC212(__this, L_9, il2cpp_rgctx_method(method->klass->rgctx_data, 14));
		return;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR int32_t List_1_get_Count_m46EEFFA770BE665EA0CB3A5332E941DA4B3C1D37_gshared_inline (List_1_t77B94703E05C519A9010DD0614F757F974E1CD8B* __this, const RuntimeMethod* method) 
{
	{
		int32_t L_0 = __this->____size;
		return L_0;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 Enumerator_get_Current_m098C9AE095DEFD00B1B0314A772330F537805EE2_gshared_inline (Enumerator_t609488A9291E792D6A59707FF2BF03FBB727E047* __this, const RuntimeMethod* method) 
{
	{
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_0 = __this->____current;
		return L_0;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void List_1_Add_m4EA3769821DBDED34CA0EBACD784AC6C1A536C1A_gshared_inline (List_1_t242CDEAEC9C92000DA96982CDB9D592DDE2AADAF* __this, Color_tD001788D726C3A7F1379BEED0260B9591F440C1F ___0_item, const RuntimeMethod* method) 
{
	ColorU5BU5D_t612261CF293F6FFC3D80AB52259FF0DC2B2CC389* V_0 = NULL;
	int32_t V_1 = 0;
	{
		int32_t L_0 = __this->____version;
		__this->____version = ((int32_t)il2cpp_codegen_add(L_0, 1));
		ColorU5BU5D_t612261CF293F6FFC3D80AB52259FF0DC2B2CC389* L_1 = __this->____items;
		V_0 = L_1;
		int32_t L_2 = __this->____size;
		V_1 = L_2;
		int32_t L_3 = V_1;
		ColorU5BU5D_t612261CF293F6FFC3D80AB52259FF0DC2B2CC389* L_4 = V_0;
		NullCheck(L_4);
		if ((!(((uint32_t)L_3) < ((uint32_t)((int32_t)(((RuntimeArray*)L_4)->max_length))))))
		{
			goto IL_0034;
		}
	}
	{
		int32_t L_5 = V_1;
		__this->____size = ((int32_t)il2cpp_codegen_add(L_5, 1));
		ColorU5BU5D_t612261CF293F6FFC3D80AB52259FF0DC2B2CC389* L_6 = V_0;
		int32_t L_7 = V_1;
		Color_tD001788D726C3A7F1379BEED0260B9591F440C1F L_8 = ___0_item;
		NullCheck(L_6);
		(L_6)->SetAt(static_cast<il2cpp_array_size_t>(L_7), (Color_tD001788D726C3A7F1379BEED0260B9591F440C1F)L_8);
		return;
	}

IL_0034:
	{
		Color_tD001788D726C3A7F1379BEED0260B9591F440C1F L_9 = ___0_item;
		List_1_AddWithResize_m7AF5556215BF2EC986B31780ACADE159F4DAC1C5(__this, L_9, il2cpp_rgctx_method(method->klass->rgctx_data, 14));
		return;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* Array_Empty_TisRuntimeObject_mFB8A63D602BB6974D31E20300D9EB89C6FE7C278_gshared_inline (const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	{
		il2cpp_codegen_runtime_class_init_inline(il2cpp_rgctx_data(method->rgctx_data, 2));
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_0 = ((EmptyArray_1_tDF0DD7256B115243AA6BD5558417387A734240EE_StaticFields*)il2cpp_codegen_static_fields_for(il2cpp_rgctx_data(method->rgctx_data, 2)))->___Value;
		return L_0;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void List_1_Add_mEBCF994CC3814631017F46A387B1A192ED6C85C7_gshared_inline (List_1_tA239CB83DE5615F348BB0507E45F490F4F7C9A8D* __this, RuntimeObject* ___0_item, const RuntimeMethod* method) 
{
	ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* V_0 = NULL;
	int32_t V_1 = 0;
	{
		int32_t L_0 = __this->____version;
		__this->____version = ((int32_t)il2cpp_codegen_add(L_0, 1));
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_1 = __this->____items;
		V_0 = L_1;
		int32_t L_2 = __this->____size;
		V_1 = L_2;
		int32_t L_3 = V_1;
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_4 = V_0;
		NullCheck(L_4);
		if ((!(((uint32_t)L_3) < ((uint32_t)((int32_t)(((RuntimeArray*)L_4)->max_length))))))
		{
			goto IL_0034;
		}
	}
	{
		int32_t L_5 = V_1;
		__this->____size = ((int32_t)il2cpp_codegen_add(L_5, 1));
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_6 = V_0;
		int32_t L_7 = V_1;
		RuntimeObject* L_8 = ___0_item;
		NullCheck(L_6);
		(L_6)->SetAt(static_cast<il2cpp_array_size_t>(L_7), (RuntimeObject*)L_8);
		return;
	}

IL_0034:
	{
		RuntimeObject* L_9 = ___0_item;
		List_1_AddWithResize_m79A9BF770BEF9C06BE40D5401E55E375F2726CC4(__this, L_9, il2cpp_rgctx_method(method->klass->rgctx_data, 14));
		return;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR int32_t List_1_get_Count_m4407E4C389F22B8CEC282C15D56516658746C383_gshared_inline (List_1_tA239CB83DE5615F348BB0507E45F490F4F7C9A8D* __this, const RuntimeMethod* method) 
{
	{
		int32_t L_0 = __this->____size;
		return L_0;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void Vector3_Normalize_mC749B887A4C74BA0A2E13E6377F17CCAEB0AADA8_inline (Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2* __this, const RuntimeMethod* method) 
{
	float V_0 = 0.0f;
	bool V_1 = false;
	{
		float L_0;
		L_0 = Vector3_get_magnitude_mF0D6017E90B345F1F52D1CC564C640F1A847AF2D_inline(__this, NULL);
		V_0 = L_0;
		float L_1 = V_0;
		V_1 = (bool)((((float)L_1) > ((float)(9.99999975E-06f)))? 1 : 0);
		bool L_2 = V_1;
		if (!L_2)
		{
			goto IL_0042;
		}
	}
	{
		float L_3 = __this->___x;
		float L_4 = V_0;
		__this->___x = ((float)(L_3/L_4));
		float L_5 = __this->___y;
		float L_6 = V_0;
		__this->___y = ((float)(L_5/L_6));
		float L_7 = __this->___z;
		float L_8 = V_0;
		__this->___z = ((float)(L_7/L_8));
		goto IL_0065;
	}

IL_0042:
	{
		__this->___x = (0.0f);
		__this->___y = (0.0f);
		__this->___z = (0.0f);
	}

IL_0065:
	{
		return;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 Vector3_op_Multiply_m87BA7C578F96C8E49BB07088DAAC4649F83B0353_inline (Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 ___0_a, float ___1_d, const RuntimeMethod* method) 
{
	Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		il2cpp_codegen_initobj((&V_0), sizeof(Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2));
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_0 = ___0_a;
		float L_1 = L_0.___x;
		float L_2 = ___1_d;
		(&V_0)->___x = ((float)il2cpp_codegen_multiply(L_1, L_2));
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_3 = ___0_a;
		float L_4 = L_3.___y;
		float L_5 = ___1_d;
		(&V_0)->___y = ((float)il2cpp_codegen_multiply(L_4, L_5));
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_6 = ___0_a;
		float L_7 = L_6.___z;
		float L_8 = ___1_d;
		(&V_0)->___z = ((float)il2cpp_codegen_multiply(L_7, L_8));
		Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2 L_9 = V_0;
		return L_9;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR float Vector3_get_sqrMagnitude_m43C27DEC47C4811FB30AB474FF2131A963B66FC8_inline (Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2* __this, const RuntimeMethod* method) 
{
	{
		float L_0 = __this->___x;
		float L_1 = __this->___x;
		float L_2 = __this->___y;
		float L_3 = __this->___y;
		float L_4 = __this->___z;
		float L_5 = __this->___z;
		return ((float)il2cpp_codegen_add(((float)il2cpp_codegen_add(((float)il2cpp_codegen_multiply(L_0, L_1)), ((float)il2cpp_codegen_multiply(L_2, L_3)))), ((float)il2cpp_codegen_multiply(L_4, L_5))));
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR float Vector3_get_magnitude_mF0D6017E90B345F1F52D1CC564C640F1A847AF2D_inline (Vector3_t24C512C7B96BBABAD472002D0BA2BDA40A5A80B2* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Math_tEB65DE7CA8B083C412C969C92981C030865486CE_il2cpp_TypeInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	{
		float L_0 = __this->___x;
		float L_1 = __this->___x;
		float L_2 = __this->___y;
		float L_3 = __this->___y;
		float L_4 = __this->___z;
		float L_5 = __this->___z;
		il2cpp_codegen_runtime_class_init_inline(Math_tEB65DE7CA8B083C412C969C92981C030865486CE_il2cpp_TypeInfo_var);
		double L_6;
		L_6 = sqrt(((double)((float)il2cpp_codegen_add(((float)il2cpp_codegen_add(((float)il2cpp_codegen_multiply(L_0, L_1)), ((float)il2cpp_codegen_multiply(L_2, L_3)))), ((float)il2cpp_codegen_multiply(L_4, L_5))))));
		return ((float)L_6);
	}
}
