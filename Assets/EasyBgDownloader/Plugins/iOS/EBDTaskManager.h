//
//  EBDTaskManager.h
//  
//
//
//
//


#ifndef EBDTaskManager_h
#define EBDTaskManager_h


@interface EBDTaskManager : NSObject

- (id)initWithSession:(NSURLSession *)session;
- (void)setTask:(NSString *)requestURL destPath:(NSString *)destPath downloadTask:(NSURLSessionDownloadTask *)downloadTask;
- (void)removeTask:(NSString *)requestURL;
- (NSString *)getUrlByTaskId:(NSInteger)taskId;
- (NSUInteger)getTaskId:(NSString *)requestURL;
- (NSString *)getDestPath:(NSInteger)taskId;
- (NSString *)getDestPathByURL:(NSString *)requestURL;
- (NSURLSessionDownloadTask *)getDownloadTask:(NSInteger)taskId;
- (NSURLSessionDownloadTask *)getDownloadTaskByURL:(NSString *)requestURL;

@end


#endif /* EBDTaskManager_h */
