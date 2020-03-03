//
//  EasyBgDownloader.mm
//  
//
//  
//
//


#import "EBDTaskManager.h"


@interface EBDTaskManager () {
    //<NSString, NSNumber>
    NSMutableDictionary *_taskIdList;
    //<NSNumber, NSURLSessionDownloadTask>
    NSMutableDictionary *_taskList;
    //<NSNumber, NSString>
    NSMutableDictionary *_destPathList;
}
@property (nonatomic, readwrite) NSURLSession *_session;
@end
static NSString *const EBD_USERDEFAULTS_TASKID_KEY_PREFIX = @"EBD_USERDEFAULTS_TASKID_";


@implementation EBDTaskManager


- (id)initWithSession:(NSURLSession *)session {
    if((self = [super init])) {
        _taskIdList = [NSMutableDictionary dictionary];
        _destPathList = [NSMutableDictionary dictionary];
        _taskList = [NSMutableDictionary dictionary];
        self._session = session;
    }
    return self;
}


/*
 * Set
 */
- (void)setTask:(NSString *)requestURL destPath:(NSString *)destPath downloadTask:(NSURLSessionDownloadTask *)downloadTask {
    NSInteger taskId = downloadTask.taskIdentifier;
    [self setTaskId:requestURL taskId:taskId];
    [self setDestPath:taskId destPath:destPath];
    [self setDownloadTask:taskId downloadTask:downloadTask];
}
- (void)setTaskId:(NSString *)requestURL taskId:(NSInteger)taskId {
    NSNumber *taskIdObj = [[NSNumber alloc] initWithInteger:taskId];
    [_taskIdList setObject:taskIdObj forKey:requestURL];
    [self setTaskIdToUD:requestURL taskId:taskId];
}
- (void)setDestPath:(NSInteger)taskId destPath:(NSString *)destPath {
    NSNumber *taskIdObj = [[NSNumber alloc] initWithInteger:taskId];
    [_destPathList setObject:destPath forKey:taskIdObj];
    [self setDestPathToUD:taskId destPath:destPath];
}
- (void)setDownloadTask:(NSInteger)taskId downloadTask:(NSURLSessionDownloadTask *)downloadTask {
    NSNumber *taskIdObj = [[NSNumber alloc] initWithInteger:taskId];
    [_taskList setObject:downloadTask forKey:taskIdObj];
}
- (void)setTaskIdToUD:(NSString *)requestURL taskId:(NSInteger)taskId {
    NSUserDefaults *userDefault = [NSUserDefaults standardUserDefaults];
    [userDefault setInteger:taskId forKey:requestURL];
    [userDefault synchronize];
}
- (void)setDestPathToUD:(NSInteger)taskId destPath:(NSString *)destPath {
    NSString *keyTaskId = [EBD_USERDEFAULTS_TASKID_KEY_PREFIX stringByAppendingString:[NSString stringWithFormat:@"%ld", taskId]];
    NSUserDefaults *userDefault = [NSUserDefaults standardUserDefaults];
    [userDefault setObject:destPath forKey:keyTaskId];
    [userDefault synchronize];
}


/*
 * Get
 */
- (NSString *)getUrlByTaskId:(NSInteger)taskId {
    NSNumber *taskIdObj = [[NSNumber alloc] initWithInteger:taskId];
    NSArray<NSString *> *keyArray = [_taskIdList allKeysForObject:taskIdObj];
    if (keyArray.count == 0) {
        NSString *requestURL = [self getUrlByTaskIdFromUD:taskId];
        if (requestURL != nil) {
            [self setTaskId:requestURL taskId:taskId];
        }
        return requestURL;
    } else {
        return keyArray.firstObject;
    }
}
- (NSUInteger)getTaskId:(NSString *)requestURL {
    NSNumber *taskIdObj = [_taskIdList objectForKey:requestURL];
    NSInteger taskId;
    
    if (taskIdObj == nil) {
        taskId = [self getTaskIdFromUD:requestURL];
        if (taskId != 0) {
            [self setTaskId:requestURL taskId:taskId];
        }
    } else {
        taskId = [taskIdObj integerValue];
    }
    return taskId;
}
- (NSString *)getDestPath:(NSInteger)taskId {
    NSNumber *taskIdObj = [[NSNumber alloc] initWithInteger:taskId];
    NSString *destPath = [_destPathList objectForKey:taskIdObj];
    
    if (destPath == nil) {
        destPath = [self getDestPathFromUD:taskId];
        if (destPath != nil) {
            [self setDestPath:taskId destPath:destPath];
        }
    }
    return destPath;
}
- (NSString *)getDestPathByURL:(NSString *)requestURL {
    NSInteger taskId = [self getTaskId:requestURL];
    if (taskId != 0) {
        return [self getDestPath:taskId];
    } else {
        return nil;
    }
}
- (NSURLSessionDownloadTask *)getDownloadTask:(NSInteger)taskId {
    NSNumber *taskIdObj = [[NSNumber alloc] initWithInteger:taskId];
    NSURLSessionDownloadTask *downloadTask = [_taskList objectForKey:taskIdObj];
    
    if (downloadTask == nil) {
        downloadTask = [self getDownloadTaskFromSession:taskId];
    }
    return downloadTask;
}
- (NSURLSessionDownloadTask *)getDownloadTaskByURL:(NSString *)requestURL {
    NSInteger taskId = [self getTaskId:requestURL];
    if (taskId != 0) {
        return [self getDownloadTask:taskId];
    } else {
        return nil;
    }
}
- (NSString *)getUrlByTaskIdFromUD:(NSInteger)taskId {
    NSDictionary *dictionary = [[NSUserDefaults standardUserDefaults] dictionaryRepresentation];
    NSNumber *taskIdObj = [[NSNumber alloc] initWithInteger:taskId];
    NSArray<NSString *> *keyArray = [dictionary allKeysForObject:taskIdObj];
    if (keyArray.count != 0) {
        return keyArray.firstObject;
    }
    return nil;
}
- (NSInteger)getTaskIdFromUD:(NSString *)requestURL {
    NSUserDefaults *userDefault = [NSUserDefaults standardUserDefaults];
    return [userDefault integerForKey:requestURL];
}
- (NSString *)getDestPathFromUD:(NSInteger)taskId {
    NSString *keyTaskId = [EBD_USERDEFAULTS_TASKID_KEY_PREFIX stringByAppendingString:[NSString stringWithFormat:@"%ld", taskId]];
    NSUserDefaults *userDefault = [NSUserDefaults standardUserDefaults];
    return [userDefault stringForKey:keyTaskId];
}
- (NSURLSessionDownloadTask *)getDownloadTaskFromSession:(NSInteger)taskId {
    if (!self._session) return nil;
    
    //Convert async to sync
    dispatch_semaphore_t semaphore = dispatch_semaphore_create(0);
    [self._session getTasksWithCompletionHandler:^(NSArray<NSURLSessionDataTask *> * _Nonnull dataTasks, NSArray<NSURLSessionUploadTask *> * _Nonnull uploadTasks, NSArray<NSURLSessionDownloadTask *> * _Nonnull downloadTasks) {
        for (NSURLSessionDownloadTask *task in downloadTasks) {
            if (taskId == task.taskIdentifier) {
                [self setDownloadTask:taskId downloadTask:task];
                break;
            }
        }
        dispatch_semaphore_signal(semaphore);
    }];
    dispatch_semaphore_wait(semaphore, DISPATCH_TIME_FOREVER);
    NSNumber *taskIdObj = [[NSNumber alloc] initWithInteger:taskId];
    return [_taskList objectForKey:taskIdObj];
}


/*
 * Remove
 */
- (void)removeTask:(NSString *)requestURL {
    NSInteger taskId = [self getTaskId:requestURL];
    
    if (taskId != 0) {
        [self removeTaskId:requestURL];
        [self removeDestPath:taskId];
        [self removeDownloadTask:taskId];
    }
}
- (void)removeTaskId:(NSString *)requestURL {
    [_taskIdList removeObjectForKey:requestURL];
    [self removeTaskIdFromUD:requestURL];
}
- (void)removeDestPath:(NSInteger)taskId {
    NSNumber *taskIdObj = [[NSNumber alloc] initWithInteger:taskId];
    [_destPathList removeObjectForKey:taskIdObj];
    [self removeDestPathFromUD:taskId];
}
- (void)removeDownloadTask:(NSInteger)taskId {
    NSNumber *taskIdObj = [[NSNumber alloc] initWithInteger:taskId];
    [_taskList removeObjectForKey:taskIdObj];
}
- (void)removeTaskIdFromUD:(NSString *)requestURL {
    NSUserDefaults *userDefault = [NSUserDefaults standardUserDefaults];
    [userDefault removeObjectForKey:requestURL];
    [userDefault synchronize];
}
- (void)removeDestPathFromUD:(NSInteger)taskId {
    NSString *keyTaskId = [EBD_USERDEFAULTS_TASKID_KEY_PREFIX stringByAppendingString:[NSString stringWithFormat:@"%ld", taskId]];
    NSUserDefaults *userDefault = [NSUserDefaults standardUserDefaults];
    [userDefault removeObjectForKey:keyTaskId];
    [userDefault synchronize];
}


/*
 * Clear
 */


@end
