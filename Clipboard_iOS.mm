#import "UnityAppController.h"

/// unity-iOS clipboard plugin


#define MakeStringCopy( _x_ ) ( _x_ != NULL && [_x_ isKindOfClass:[NSString class]] ) ? strdup( [_x_ UTF8String] ) : NULL
#define GetStringParam( _x_ ) ( _x_ != NULL ) ? [NSString stringWithUTF8String:_x_] : [NSString stringWithUTF8String:""]

//[UIPasteboard generalPasteboard].string = @"the text to copy";
//NSString *whatsOnThePasteboard = [UIPasteboard generalPasteboard].string;

//send clipboard to unity
extern "C" const char * _importString()
{
    NSString *result = [UIPasteboard generalPasteboard].string;
    return MakeStringCopy(result);
}

//get clipboard from unity
extern "C" void _exportString(const char * eString)
{
    [UIPasteboard generalPasteboard].string = GetStringParam(eString);  //@"the text to copy";
}
