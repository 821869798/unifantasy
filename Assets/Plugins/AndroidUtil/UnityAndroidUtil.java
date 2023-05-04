package com.unitygame;

import android.app.Activity;
import android.os.Environment;
import android.os.StatFs;

import com.unity3d.player.UnityPlayer;

import java.io.File;
import java.lang.reflect.Method;

public class UnityAndroidUtil {
    // 全面屏刘海屏适配
    /**
     * OPPO
     *
     * @return hasNotch
     */
    public static boolean HasNotchInOppo() {
        final Activity act = UnityPlayer.currentActivity;
        return act.getPackageManager().hasSystemFeature("com.oppo.feature.screen.heteromorphism");
    }

    /**
     * VIVO
     * <p>
     * android.util.FtFeature
     * public static boolean isFeatureSupport(int mask);
     * <p>
     * 参数:
     * 0x00000020表示是否有凹槽;
     * 0x00000008表示是否有圆角。
     *
     * @return hasNotch
     */
    public static boolean HasNotchInVivo() {
        boolean hasNotch = false;
        final Activity act = UnityPlayer.currentActivity;
        try {
            ClassLoader cl = act.getClassLoader();
            Class<?> FtFeature = cl.loadClass("android.util.FtFeature");
            Method get = FtFeature.getMethod("isFeatureSupport",int.class);
            hasNotch = (boolean) get.invoke(FtFeature, new Object[]{0x00000020});
        } catch (Exception e) {
            //e.printStackTrace();
        }
        return hasNotch;
    }

    /**
     * HUAWEI
     * com.huawei.android.util.HwNotchSizeUtil
     * public static boolean hasNotchInScreen()
     *
     * @return hasNotch
     */
    public static boolean HasNotchInHuawei() {
        boolean hasNotch = false;
        final Activity act = UnityPlayer.currentActivity;
        try {
            ClassLoader cl = act.getClassLoader();
            Class<?> HwNotchSizeUtil = cl.loadClass("com.huawei.android.util.HwNotchSizeUtil");
            Method get = HwNotchSizeUtil.getMethod("hasNotchInScreen");
            hasNotch = (boolean) get.invoke(HwNotchSizeUtil);
        } catch (Exception e) {
            //e.printStackTrace();
        }
        return hasNotch;
    }

    /**
     * 小米
     * @return
     */
    public static boolean HasNotchInXiaomi(){
        if(getSystemPropertiesInt("ro.miui.notch",0) == 1){
            return true;
        }
        return false;
    }

    public static int getSystemPropertiesInt(String key, int def) {
        try {
            Method getIntMethod = Class.forName("android.os.SystemProperties").getMethod("getInt", new Class[] {String.class, Integer.TYPE});
            return  ((Integer)getIntMethod.invoke(null, key, def)).intValue();
        } catch(Exception localException1) {
        }
        return def;
    }

    public static int GetStatusHeight() {
        final Activity act = UnityPlayer.currentActivity;
        int result = 0x0;
        int resourceId = act.getResources().getIdentifier("status_bar_height", "dimen", "android");
        if(resourceId > 0) {
            result = act.getResources().getDimensionPixelSize(resourceId);
        }
        return result;
    }

    public static long GetAvailableSize()
    {
        File path = Environment.getDataDirectory();
        StatFs stat = new StatFs(path.getPath());
        long availableBytes = 0;
        if(android.os.Build.VERSION.SDK_INT >= 18)
        {
            availableBytes = stat.getAvailableBytes();
        }
        else
        {
            long blockSize = stat.getBlockSize();
            long totalBlocks = stat.getBlockCount();
            availableBytes =  totalBlocks * blockSize;
        }
        return availableBytes;
    }
}
