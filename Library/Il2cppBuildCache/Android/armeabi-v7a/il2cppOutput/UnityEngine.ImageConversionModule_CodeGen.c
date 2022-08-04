#include "pch-c.h"
#ifndef _MSC_VER
# include <alloca.h>
#else
# include <malloc.h>
#endif


#include "codegen/il2cpp-codegen-metadata.h"





// 0x00000001 System.Byte[] UnityEngine.ImageConversion::EncodeToPNG(UnityEngine.Texture2D)
extern void ImageConversion_EncodeToPNG_mA598C2969C878ACC5AEA5FDC92C6199EB30D51ED (void);
// 0x00000002 System.Byte[] UnityEngine.ImageConversion::EncodeToJPG(UnityEngine.Texture2D,System.Int32)
extern void ImageConversion_EncodeToJPG_mE32249F45E643405652057EA8A35112AB4721641 (void);
static Il2CppMethodPointer s_methodPointers[2] = 
{
	ImageConversion_EncodeToPNG_mA598C2969C878ACC5AEA5FDC92C6199EB30D51ED,
	ImageConversion_EncodeToJPG_mE32249F45E643405652057EA8A35112AB4721641,
};
static const int32_t s_InvokerIndices[2] = 
{
	5947,
	5380,
};
extern const CustomAttributesCacheGenerator g_UnityEngine_ImageConversionModule_AttributeGenerators[];
IL2CPP_EXTERN_C const Il2CppCodeGenModule g_UnityEngine_ImageConversionModule_CodeGenModule;
const Il2CppCodeGenModule g_UnityEngine_ImageConversionModule_CodeGenModule = 
{
	"UnityEngine.ImageConversionModule.dll",
	2,
	s_methodPointers,
	0,
	NULL,
	s_InvokerIndices,
	0,
	NULL,
	0,
	NULL,
	0,
	NULL,
	NULL,
	g_UnityEngine_ImageConversionModule_AttributeGenerators,
	NULL, // module initializer,
	NULL,
	NULL,
	NULL,
};
