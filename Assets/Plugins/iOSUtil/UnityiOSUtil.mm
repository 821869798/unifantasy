
extern "C" {

    long long _IOS_GetFreeDiskSpace()
    {
        
        if(@available(iOS 11.0, *)) {
            
            NSURL *fileURL = [[NSURL alloc] initFileURLWithPath:NSTemporaryDirectory()];
            NSDictionary *results = [fileURL resourceValuesForKeys:@[NSURLVolumeAvailableCapacityForImportantUsageKey] error:nil];
            
            long long space = [results[NSURLVolumeAvailableCapacityForImportantUsageKey]  longLongValue];
            
            return space;
            
        }else{
            NSError * error = nil;
            
            NSDictionary * systemAttributes = [[NSFileManager defaultManager] attributesOfFileSystemForPath:NSHomeDirectory() error:&error];
            
            if(error) {
                
                return 0;
                
            }
            
            long long space = [systemAttributes[NSFileSystemFreeSize] longLongValue];
            
            return space;
        }
        
    }

}
