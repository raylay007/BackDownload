//
//  EBDInterface.mm
//  
//
//
//
//

#import <Foundation/Foundation.h>
#include "EasyBgDownloader.h"


static NSString *_productName = @"SampleProduct";
static NSString *_gameObjName = @"GameObject";
static BOOL _cacheEnabled = NO;
static BOOL _notificationEnabled = YES;
static EasyBgDownloader *_Downloader;

static EasyBgDownloader *_GetDownloader() {
    if (!_Downloader) {
        _Downloader = [[EasyBgDownloader alloc] initWithGameObjName:_gameObjName cacheEnabled:_cacheEnabled notificationEnabled:_notificationEnabled];
    }
    return _Downloader;
}


/*
 * Interface
 */
extern "C" void EBDInterfaceInit(const char *gameObjName, bool cacheEnabled, bool notificationEnabled) {
    _gameObjName = [[NSString alloc] initWithUTF8String:gameObjName];
    _cacheEnabled = cacheEnabled;
    _notificationEnabled = notificationEnabled;
    
    [_GetDownloader() initEBD];
}
extern "C" void EBDInterfaceTerminate() {
    [_GetDownloader() terminateEBD];
}
extern "C" void EBDInterfaceResume() {
    [_GetDownloader() resumeEBD];
}
extern "C" void EBDInterfacePause() {
    [_GetDownloader() pauseEBD];
}
extern "C" void EBDInterfaceStartDL(const char *requestURL, const char *destPath) {
    [_GetDownloader() startDL:[[NSString alloc] initWithUTF8String:requestURL] destPath:[[NSString alloc] initWithUTF8String:destPath]];
}
extern "C" void EBDInterfaceStopDL(const char *requestURL) {
    [_GetDownloader() stopDL:[[NSString alloc] initWithUTF8String:requestURL]];
}
extern "C" int EBDInterfaceGetStatus(const char *requestURL) {
    return [_GetDownloader() getStatus:[[NSString alloc] initWithUTF8String:requestURL]];
}
extern "C" float EBDInterfaceGetProgress(const char *requestURL) {
    return [_GetDownloader() getProgress:[[NSString alloc] initWithUTF8String:requestURL]];
}


/*
 * Test
 */
extern "C" void EBDTestVoid() {
    NSLog(@"Log fron iOS Native void");
    UnitySendMessage("Downloader", "CallUnitySendMessage", "Native");
    return;
}
extern "C" int EBDTestReturnInt() {
    NSLog(@"Log fron iOS Native Return Int");
    return 1;
}
extern "C" void EBDTestArgInt(int i) {
    NSLog(@"Log fron iOS Native Value Int");
    return;
}
