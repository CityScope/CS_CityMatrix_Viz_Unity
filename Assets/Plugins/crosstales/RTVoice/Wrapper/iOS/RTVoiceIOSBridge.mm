//
//  RTVoiceIOSBridge.mm
//  Version 2.8.6
//
//  Copyright 2016-2017 www.crosstales.com
//
#import "RTVoiceIOSBridge.h"
#import <AVFoundation/AVFoundation.h>
#import <Foundation/Foundation.h>

@interface RTVoiceIOSBridge () <AVSpeechSynthesizerDelegate>
@property (readwrite, nonatomic, strong) AVSpeechSynthesizer *synthesizer;
@end

@implementation RTVoiceIOSBridge
@synthesize synthesizer = _synthesizer;

AVSpeechSynthesizer *MySynthesizer;

- (AVSpeechSynthesizer *)synthesizer
{
    if (!_synthesizer)
    {
        _synthesizer = [[AVSpeechSynthesizer alloc] init];
        _synthesizer.delegate = self;
    }
    return _synthesizer;
}


/**
 * Speaks the string with a given rate, pitch, volume and culture.
 * @param name Name of the voice to speak
 * @param text Text to speak
 * @param rate Speech rate of the speaker in percent
 * @param pitch Pitch of the speech in percent
 * @param volume Volume of the speaker in percent
 */
- (void)speak: (NSString *)name text:(NSString *)text rate:(float)rate pitch:(float)pitch volume:(float)volume
//- (void)speak: (NSString *)id text:(NSString *)text rate:(float)rate pitch:(float)pitch volume:(float)volume
{
#ifdef DEBUG
    NSLog(@"speak: %@ - Text: %@, Rate: %.3f, Pitch: %.3f, Volume: %.3f", name, text, rate, pitch, volume);
    //NSLog(@"speak: %@ - Text: %@, Rate: %.3f, Pitch: %.3f, Volume: %.3f", id, text, rate, pitch, volume);
#endif

    if (!_synthesizer)
    {
        _synthesizer = [[AVSpeechSynthesizer alloc] init];
        _synthesizer.delegate = self;
    }
    
    [MySynthesizer stopSpeakingAtBoundary:AVSpeechBoundaryImmediate];
    
    if (text)
    {
        NSArray *voices = [AVSpeechSynthesisVoice speechVoices];

        AVSpeechSynthesisVoice *voice = voices[0]; // one voice must be available
        
        for (AVSpeechSynthesisVoice *v in voices) {
            if ([v.name isEqualToString:name])
            //if ([v.identifier isEqualToString:id])
            {
                voice = v;
                break;
            }
        }

#ifdef DEBUG
        NSLog(@"speak - selected voice: %@", voice.name);
#endif

        AVSpeechUtterance *utterance = [[AVSpeechUtterance alloc] initWithString:text];
        utterance.voice = voice;

        float adjustedRate = AVSpeechUtteranceDefaultSpeechRate * rate;
        
        if (adjustedRate > AVSpeechUtteranceMaximumSpeechRate)
        {
            adjustedRate = AVSpeechUtteranceMaximumSpeechRate;
        }

        if (adjustedRate < AVSpeechUtteranceMinimumSpeechRate)
        {
            adjustedRate = AVSpeechUtteranceMinimumSpeechRate;
        }

        utterance.rate = adjustedRate;
        utterance.volume = volume;

        utterance.pitchMultiplier = pitch;

        MySynthesizer = _synthesizer;
        [_synthesizer speakUtterance:utterance];
    } else {
        NSLog(@"WARNING: text was null - could not speak!");
    }
}

/**
 * Stops speaking
 */
- (void)stop
{
#ifdef DEBUG
    NSLog(@"stop");
#endif

    [MySynthesizer stopSpeakingAtBoundary:AVSpeechBoundaryImmediate];
}

/** 
 * Collects and sends all voices to RTVoice.
 */
- (void)setVoices
{
    NSArray *voices = [AVSpeechSynthesisVoice speechVoices];
    
    NSString *appendstring = @"";
    for (AVSpeechSynthesisVoice *voice in voices) {
        //appendstring = [appendstring stringByAppendingString:voice.identifier];
        //appendstring = [appendstring stringByAppendingString:@","];
        appendstring = [appendstring stringByAppendingString:voice.name];
        appendstring = [appendstring stringByAppendingString:@","];
        appendstring = [appendstring stringByAppendingString:voice.language];
        appendstring = [appendstring stringByAppendingString:@","];
        
#ifdef DEBUG
        NSLog(@"Voice-ID: %@ - Name: %@, Language: %@, Quality: %ld", voice.identifier, voice.name, voice.language, (long)voice.quality);
#endif
    }
    
#ifdef DEBUG
    NSLog(@"setVoices: %@", appendstring);
#endif

    UnitySendMessage("RTVoice", "SetVoices", [appendstring UTF8String]);
}

/**
 * Called when the speak is finished and informs RTVoice.
 */
- (void)speechSynthesizer:(AVSpeechSynthesizer *)synthesizer didFinishSpeechUtterance:(AVSpeechUtterance *)utterance
{
#ifdef DEBUG
    NSLog(@"didFinishSpeechUtterance");
#endif

    UnitySendMessage("RTVoice", "SetState", "Finish");
}

/** 
 * Called when the synthesizer have began to speak a word and informs RTVoice.
 */
- (void)speechSynthesizer:(AVSpeechSynthesizer *)synthesizer willSpeakRangeOfSpeechString:(NSRange)characterRange utterance:(AVSpeechUtterance *)utterance
{
#ifdef DEBUG
    NSLog(@"willSpeakRangeOfSpeechString");
#endif

    UnitySendMessage("RTVoice", "WordSpoken", "w");//[substringcutout UTF8String]);
}

/**
 * Called when the speak is canceled and informs RTVoice.
 */
- (void)speechSynthesizer:(AVSpeechSynthesizer *)synthesizer didCancelSpeechUtterance:(AVSpeechUtterance *)utterance
{
#ifdef DEBUG
    NSLog(@"didCancelSpeechUtterance");
#endif

    UnitySendMessage("RTVoice", "SetState", "Cancel");
}

/**
 * Called when the speak is started and informs RTVoice.
 */
- (void)speechSynthesizer:(AVSpeechSynthesizer *)synthesizer didStartSpeechUtterance:(AVSpeechUtterance *)utterance
{
#ifdef DEBUG
    NSLog(@"didStartSpeechUtterance");
#endif

    UnitySendMessage("RTVoice", "SetState", "Start");
}

/**
 * Called when the application finished launching
 */
/*
- (void)applicationDidFinishLaunching:(NSNotification *)aNotification
{
    [self setVoices];
}
*/


@end

extern void sendMessage(const char *, const char *, const char *);

extern "C" {
    
    /**
     * Bridge to speak the string that it receives with a given rate, pitch, volume and identifier.
     * @param name Name of the voice to speak
     * @param text Text to speak
     * @param rate Speech rate of the speaker in percent
     * @param pitch Pitch of the speech in percent
     * @param volume Volume of the speaker in percent
     */
    void Speak(char *name, char *text, float rate, float pitch, float volume)
    //void Speak(char *id, char *text, float rate, float pitch, float volume)
    {
        if([[[UIDevice currentDevice]systemVersion]floatValue] < 8){
            NSLog(@"ERROR: RT-Voice doesn't support iOS-versions before 8!");
        } else {
            NSString *voiceName = [NSString stringWithUTF8String:name];
            //NSString *voiceId = [NSString stringWithUTF8String:id];
            NSString *messageFromRTVoice = [NSString stringWithUTF8String:text];

#ifdef DEBUG
            NSLog(@"Speak: %@ - Text: %@, Rate: %.3f, Pitch: %.3f, Volume: %.3f", voiceName, messageFromRTVoice, rate, pitch, volume);
            //NSLog(@"Speak: %@ - Text: %@, Rate: %.3f, Pitch: %.3f, Volume: %.3f", voiceId, messageFromRTVoice, rate, pitch, volume);
#endif

            [[RTVoiceIOSBridge alloc] speak:voiceName text:messageFromRTVoice rate:rate pitch:pitch volume:volume];
            //[[RTVoiceIOSBridge alloc] speak:voiceId text:messageFromRTVoice rate:rate pitch:pitch volume:volume];
        }
    }
    
    /**
     * Bridge to stop speaking.
     */
    void Stop()
    {
        if([[[UIDevice currentDevice]systemVersion]floatValue] < 8){
            NSLog(@"ERROR: RT-Voice doesn't support iOS-versions before 8!");
        } else {
#ifdef DEBUG
            NSLog(@"Stop");
#endif

            [[RTVoiceIOSBridge alloc] stop];
        }
    }
    
    /** 
     * Bridge to get all voices.
     */
    void GetVoices()
    {
        if([[[UIDevice currentDevice]systemVersion]floatValue] < 8){
            NSLog(@"ERROR: RT-Voice doesn't support iOS-versions before 8!");
        } else {
#ifdef DEBUG
            NSLog(@"GetVoices");
#endif

            [[RTVoiceIOSBridge alloc] setVoices];
        }
    }
}