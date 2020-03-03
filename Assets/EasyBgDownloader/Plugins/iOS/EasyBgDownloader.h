//
//  EasyBgDownloader.h
//  
//
//
//
//


#ifndef EasyBgDownloader_h
#define EasyBgDownloader_h


@interface EasyBgDownloader : NSObject

- (id)initWithGameObjName:(NSString *)gameObjName cacheEnabled:(BOOL)cacheEnabled notificationEnabled:(BOOL)notificationEnabled;
- (void)initEBD;
- (void)terminateEBD;
- (void)resumeEBD;
- (void)pauseEBD;
- (void)startDL:(NSString *)requestURL destPath:(NSString *)destPath;
- (void)stopDL:(NSString *)requestURL;
- (int)getStatus:(NSString *)requestURL;
- (float)getProgress:(NSString *)requestURL;

@end


#endif /* EasyBgDownloader_h */
