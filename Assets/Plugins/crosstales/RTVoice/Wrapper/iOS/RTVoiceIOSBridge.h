//
//  RTVoiceIOSBridge.h
//  Version 2.8.6
//
//  Copyright 2016-2017 www.crosstales.com
//
#ifndef RTVoiceIOSBridge_h
#define RTVoiceIOSBridge_h

@interface RTVoiceIOSBridge:NSObject
- (void)setVoices;
- (void)speak:(NSString *)name text:(NSString *)text rate:(float)rate pitch:(float)pitch volume:(float)volume;
//- (void)speak:(NSString *)id text:(NSString *)text rate:(float)rate pitch:(float)pitch volume:(float)volume;
- (void)stop;
@end


#ifdef __cplusplus
extern "C" {
    
    void UnitySendMessage(const char *, const char *, const char *);
    
}
#endif

#endif