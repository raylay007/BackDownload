//
//  EBDAppDeletate.mm
//  
//
//
//
//


#import "EBDAppDeletate.h"
#import "EasyBgDownloader.h"


@interface EBDAppDeletate () <UIApplicationDelegate> {

}
@end


@implementation EBDAppDeletate

- (void)registerLocalNotification:(UIApplication *)application {
    if ([application respondsToSelector:@selector(registerUserNotificationSettings:)]) {
        UIUserNotificationSettings *settings = [UIUserNotificationSettings settingsForTypes:UIUserNotificationTypeAlert|UIUserNotificationTypeSound categories:nil];
        [application registerUserNotificationSettings:settings];
    }
}

#pragma mark -- UIApplicationDelegate --

- (BOOL)application:(UIApplication *)application didFinishLaunchingWithOptions:(NSDictionary *)launchOptions {
    BOOL valueOfSuper = [super application:application didFinishLaunchingWithOptions:launchOptions];
    
    [self registerLocalNotification:application];
    
    return valueOfSuper;
}

- (void)application:(UIApplication *)application handleEventsForBackgroundURLSession:(NSString *)identifier completionHandler:(void (^)())completionHandler {
    //[super application:application handleEventsForBackgroundURLSession:identifier completionHandler:(void (^)())completionHandler];
    
    NSLog(@"Call : handleEventsForBackgroundURLSession In App Delegate");
}
@end


IMPL_APP_CONTROLLER_SUBCLASS(EBDAppDeletate)