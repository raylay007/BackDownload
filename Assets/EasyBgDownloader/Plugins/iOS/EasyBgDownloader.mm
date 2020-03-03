//
//  EasyBgDownloader.mm
//  
//
//
//
//

#import "EasyBgDownloader.h"
#import "EBDTaskManager.h"


@interface EasyBgDownloader () <NSURLSessionDelegate, NSURLSessionTaskDelegate, NSURLSessionDownloadDelegate> {
    NSString *_currentRequestURL;
    NSURLSessionDownloadTask *_currentDownloadTask;
    float _currentProgress;
    //EBDTaskManager *_taskManager;
    
    NSString *_gameObjName;
    BOOL _cacheEnabled;
    BOOL _notificationEnabled;
}
typedef NS_ENUM (int, EBDUnityStatus) {
    UnityStatusNotInQueue = -100,
    UnityStatusPending = 10,
    UnityStatusRunning = 20,
    UnityStatusPaused = 30,
    UnityStatusFailed = 40
};
@property (nonatomic, readwrite) NSURLSession *session;
@property (nonatomic, readwrite) EBDTaskManager *_taskManager;
@end

static NSString *const EBD_SESSION_ID_PREFIX = @"EBD_SESSION_";
static NSString *const ON_COMPLETE_UNITY_METHOD = @"onCompleteDL";

@implementation EasyBgDownloader

/*
 * override init functions
 */
- (id)initWithGameObjName:(NSString *)gameObjName cacheEnabled:(BOOL)cacheEnabled notificationEnabled:(BOOL)notificationEnabled {
    if ((self = [self init])) {
        _gameObjName = gameObjName;
        _cacheEnabled = cacheEnabled;
        _notificationEnabled = notificationEnabled;
    }
    return self;
}
- (id)init {
    if ((self = [super init])) {
        _currentRequestURL = nil;
        _currentDownloadTask = nil;
        _currentProgress = 0.0f;
    }
    return self;
}

- (void)initEBD {
    
}
- (void)terminateEBD {
    
}
- (void)resumeEBD {
    
}
- (void)pauseEBD {
    
}


/*
 * Donwload Controls
 */
- (void)startDL:(NSString *)requestURL destPath:(NSString *)destPath {
    NSURL *url = [NSURL URLWithString:requestURL];
    NSURLSessionDownloadTask *downloadTask = [[self getSession] downloadTaskWithURL:url];
    
    [downloadTask resume];
    [[self getTaskManager] setTask:requestURL destPath:destPath downloadTask:downloadTask];
}

- (void)stopDL:(NSString *)requestURL {
    NSURLSessionDownloadTask *downloadTask = [[self getTaskManager] getDownloadTaskByURL:requestURL];
    if (downloadTask == nil) return;
    
    if ([_currentRequestURL isEqualToString:requestURL]) {
        [self initCurrentTask];
    }
    
    [downloadTask cancel];
    [[self getTaskManager] removeTask:requestURL];
}


/*
 * Donwload Status
 */
- (int)getStatus:(NSString *)requestURL {
    int status = UnityStatusNotInQueue;
    NSURLSessionDownloadTask *downloadTask = [[self getTaskManager] getDownloadTaskByURL:requestURL];
    if (downloadTask == nil) return status;
    
    switch (downloadTask.state) {
        case NSURLSessionTaskStateRunning:
            status = UnityStatusRunning;
            break;
        case NSURLSessionTaskStateSuspended:
            status = UnityStatusPaused;
            break;
        case NSURLSessionTaskStateCompleted:
            status = UnityStatusNotInQueue;
            break;
        case NSURLSessionTaskStateCanceling:
            status = UnityStatusNotInQueue;
            break;
        default:
            status = UnityStatusNotInQueue;
            break;
    }
    return status;
}


/*
 * Donwload Progress
 */
- (float)getProgress:(NSString *)requestURL {
    if (!_currentRequestURL || ![_currentRequestURL isEqualToString:requestURL]) {
        [self changeCurrentTaskByURL:requestURL];
    }
    
    if (!_currentDownloadTask || _currentDownloadTask.state == NSURLSessionTaskStateCompleted || _currentDownloadTask.state == NSURLSessionTaskStateCanceling) {
        return 0.0f;
    }
    
    return _currentProgress;
}


/*
 * Download Event
 */
- (void)onComplete:(NSInteger)taskId {
    NSString *requestURL = [[self getTaskManager] getUrlByTaskId:taskId];
    
    if (requestURL) {
        NSString *destPath = [[self getTaskManager] getDestPath:taskId];
        [[self getTaskManager] removeTask:requestURL];
        
        if ([_currentRequestURL isEqualToString:requestURL]) {
            [self initCurrentTask];
        }
        
        if (destPath) {
            [self presentLocalNotification:@"Finish Download!"];
            NSString *taskInfo = [NSString stringWithFormat:@"%@,%@", requestURL, destPath];
            UnitySendMessage([_gameObjName UTF8String], [ON_COMPLETE_UNITY_METHOD UTF8String], [taskInfo UTF8String]);
        }
    }
}

- (void)onFailed:(NSInteger)taskId {
    
}


/*
 * Platform Specific
 */
- (NSURLSession *)getSession {
    if (!self.session) {
        NSString *sessionIdentifier = [EBD_SESSION_ID_PREFIX stringByAppendingString:[[NSBundle mainBundle] bundleIdentifier]];
        NSURLSessionConfiguration *configuration = [NSURLSessionConfiguration backgroundSessionConfigurationWithIdentifier:sessionIdentifier];
        
        self.session = [NSURLSession sessionWithConfiguration:configuration delegate:self delegateQueue:nil];
    }
    return self.session;
}

- (EBDTaskManager *)getTaskManager {
    if (!self._taskManager) {
        self._taskManager = [[EBDTaskManager alloc] initWithSession:[self getSession]];
    }
    return self._taskManager;
}

- (void)changeCurrentTaskByURL:(NSString *)requestURL {
    _currentRequestURL = requestURL;
    _currentDownloadTask = [[self getTaskManager] getDownloadTaskByURL:requestURL];
    _currentProgress = 0.0f;
}

- (void)initCurrentTask {
    _currentRequestURL = nil;
    _currentDownloadTask = nil;
    _currentProgress = 0.0f;
}

- (BOOL)copyFileTpPath:(NSURL *)dataLocation destPath:(NSString *)destURLStr {
    NSURL *destURL = [NSURL URLWithString:destURLStr];
    NSFileManager *fileManager = [NSFileManager defaultManager];
    [fileManager removeItemAtURL:destURL error:nil];
    BOOL copied = [fileManager copyItemAtURL:dataLocation toURL:destURL error:nil];
    return copied;
}

- (void)presentLocalNotification:(NSString *)message {
    if (_notificationEnabled && [EasyBgDownloader isAppInBackground]) {
        UILocalNotification* localNotification = [[UILocalNotification alloc] init];
        localNotification.alertBody = message;
        localNotification.alertAction = @"OK";
        localNotification.soundName = UILocalNotificationDefaultSoundName;
        [[UIApplication sharedApplication] presentLocalNotificationNow:localNotification];
    }
}

+ (BOOL)isAppInBackground {
    UIApplicationState appState = [[UIApplication sharedApplication] applicationState];
    if (appState == UIApplicationStateBackground) {
        return YES;
    } else {
        return NO;
    }
}


/*
 * Delegate functions
 */
#pragma mark -- NSURLSessionDelegate --

- (void)URLSessionDidFinishEventsForBackgroundURLSession:(NSURLSession *)session {
    
}

#pragma mark -- NSURLSessionTaskDelegate --

- (void)URLSession:(NSURLSession *)session task:(NSURLSessionTask *)task didCompleteWithError:(NSError *)error {
    if (error) {
        //TODO : Error Handling
        NSLog(@"Download failed");
        [self onFailed:task.taskIdentifier];
    }
}

#pragma mark -- NSURLSessionDownloadDelegate --

- (void)URLSession:(NSURLSession *)session downloadTask:(NSURLSessionDownloadTask *)downloadTask didFinishDownloadingToURL:(NSURL *)location {
    NSData *data = [NSData dataWithContentsOfURL:location];
    if (data.length == 0) {
        //TODO : Error Handling
        NSLog(@"Download failed cause no data.");
        [self onFailed:downloadTask.taskIdentifier];
        return;
    }
    
    NSString *requestURL = [downloadTask.originalRequest.URL absoluteString];
    NSInteger taskId = downloadTask.taskIdentifier;
    /*
    NSInteger taskId = [[self getTaskManager] getTaskId:requestURL];
    if (taskId == 0) {
        taskId = downloadTask.taskIdentifier;
    }
    */
    NSString *destPath = [[self getTaskManager] getDestPath:taskId];
    
    if (!destPath) {
        //TODO : Error Handling
        NSLog(@"Avoided saving cause there is no dest path and remove Task");
        [[self getTaskManager] removeTask:requestURL];
        return;
    }
    
    //[self saveFileWithPath:data localPath:destPath];
    [self copyFileTpPath:location destPath:destPath];
    [self onComplete:taskId];
}

- (void)URLSession:(NSURLSession *)session downloadTask:(NSURLSessionDownloadTask *)downloadTask didWriteData:(int64_t)bytesWritten totalBytesWritten:(int64_t)totalBytesWritten totalBytesExpectedToWrite:(int64_t)totalBytesExpectedToWrite {
    if (_currentDownloadTask == nil || _currentDownloadTask != downloadTask) return;
    
    _currentProgress = (float)((double)totalBytesWritten / (double)totalBytesExpectedToWrite);
}

- (void)URLSession:(NSURLSession *)session downloadTask:(NSURLSessionDownloadTask *)downloadTask didResumeAtOffset:(int64_t)fileOffset expectedTotalBytes:(int64_t)expectedTotalBytes {
}


@end
