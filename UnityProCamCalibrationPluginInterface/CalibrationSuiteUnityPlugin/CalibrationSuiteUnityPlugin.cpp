// CalibrationSuiteUnityPlugin.cpp : Defines the exported functions for the DLL application.
//

#include "stdafx.h"
#include "CalibrationSuiteController.h"

extern "C"
{
    // --------------------------------------------------------------------------
    // SetTimeFromUnity, an example function we export which is called by one of the scripts.

    static float g_Time;

    static CalibrationSuiteController* s_CalibrationAPI;
    static UnityGfxRenderer s_DeviceType = kUnityGfxRendererNull;


    void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API SetTimeFromUnity(float t)
    {
        g_Time = t;
    }

    // --------------------------------------------------------------------------
    // UnitySetInterfaces

    static void UNITY_INTERFACE_API OnGraphicsDeviceEvent(UnityGfxDeviceEventType eventType);

    static IUnityInterfaces* s_UnityInterfaces = NULL;
    static IUnityGraphics* s_Graphics = NULL;

    void	UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API UnityPluginLoad(IUnityInterfaces* unityInterfaces)
    {
        s_UnityInterfaces = unityInterfaces;
        s_Graphics = s_UnityInterfaces->Get<IUnityGraphics>();
        s_Graphics->RegisterDeviceEventCallback(OnGraphicsDeviceEvent);

        // Run OnGraphicsDeviceEvent(initialize) manually on plugin load
        OnGraphicsDeviceEvent(kUnityGfxDeviceEventInitialize);
    }

    void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API UnityPluginUnload()
    {
        s_Graphics->UnregisterDeviceEventCallback(OnGraphicsDeviceEvent);
    }


    // --------------------------------------------------------------------------
    // GraphicsDeviceEvent

    static void UNITY_INTERFACE_API OnGraphicsDeviceEvent(UnityGfxDeviceEventType eventType)
    {
        // Create graphics API implementation upon initialization
        if (eventType == kUnityGfxDeviceEventInitialize)
        {
            assert(s_CalibrationAPI == NULL);
            s_DeviceType = s_Graphics->GetRenderer();
            s_CalibrationAPI = new CalibrationSuiteController();
        }

        // Let the implementation process the device related events
        if (s_CalibrationAPI)
        {
            s_CalibrationAPI->ProcessDeviceEvent(eventType, s_UnityInterfaces);
        }

        // Cleanup graphics API implementation upon shutdown
        if (eventType == kUnityGfxDeviceEventShutdown)
        {
			if (s_CalibrationAPI != nullptr)
				delete s_CalibrationAPI;

            s_CalibrationAPI = NULL;
            s_DeviceType = kUnityGfxRendererNull;
        }
    }

    // --------------------------------------------------------------------------
    // GetRenderEventFunc, an example function we export which is used to get a rendering event callback function.

    static void UNITY_INTERFACE_API OnRenderEvent(int eventID)
    {
        // Unknown / unsupported graphics device type? Do nothing
        if (s_CalibrationAPI == NULL)
            return;

        s_CalibrationAPI->RenderAllProjectors();

    }

    UnityRenderingEvent UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API GetRenderEventFunc()
    {
        s_CalibrationAPI->CheckProjectorAllCallbacks();

        return OnRenderEvent;
    }

    // --------------------------------------------------------------------------
    // Configuration 
    bool UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API Configure(const char* _ConfigPath)
    {
        if (!s_CalibrationAPI) return false;
        if (s_CalibrationAPI->IsConfigured()) return true;

        return s_CalibrationAPI->Configure(_ConfigPath);
    }

    void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API ShowDisplays(bool state)
    {
        if (!s_CalibrationAPI) return;
        s_CalibrationAPI->EnableFullScreen(-1, state);
        s_CalibrationAPI->IdentifyProjector(-1, state);
    }

    void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API OnUnityStart() 
    {
        if (!s_CalibrationAPI) return;
        s_CalibrationAPI->OnUnityStart();
    }

    void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API GetServoReconstructionFile(char* servoFile, int len)
    {
        if (!s_CalibrationAPI) return;

        std::string file = s_CalibrationAPI->GetServoReconstructionFile();
        int capcity = (file.size() < len) ? file.size() * sizeof(char) : len;


        strcpy_s(servoFile, len, file.c_str());
    }

	bool UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API IsProjectConfigured()
	{
		if (!s_CalibrationAPI) return false;
		return s_CalibrationAPI->IsConfigured();
	}


    // --------------------------------------------------------------------------
    // Projector API
    void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API EnableFullScreenForProjector(int _id, bool state) 
    {
        if (!s_CalibrationAPI) return;
        s_CalibrationAPI->EnableFullScreen(_id, state);
    }

    void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API DisplayGridAndIdentifyProjector(int _id, bool state)
    {
        if (!s_CalibrationAPI) return;
        s_CalibrationAPI->IdentifyProjector(_id, state);
    }

    void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API SetTextureForProjectorFromUnity(int _id, void* _textureHandle, int _w, int _h)
    {
        if (!s_CalibrationAPI) return;
        s_CalibrationAPI->SetProjectorTexture(_id, _textureHandle, _w, _h);
    }

    void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API SetProjectorOnResizeCallback(int _id, cslib::OnResizeCallback cb)
    {
        if (!s_CalibrationAPI) return;
        s_CalibrationAPI->SetProjectorCallback(_id, cb);
    }

    ProjectorDescriptor UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API GetProjectorProjectorDetails(int _id)
    {
        if (!s_CalibrationAPI) return ProjectorDescriptor();

        return s_CalibrationAPI->GetProjectorDescriptor(_id);
    }   

    // --------------------------------------------------------------------------
    // Servo API

    ServoRange UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API GetServoMovementRangeInDegrees(int _id)
    {
        if (!s_CalibrationAPI) return ServoRange();
        return s_CalibrationAPI->GetServoRange(_id);
    }

    void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API RecalibrateAllServos()
    {
        if (!s_CalibrationAPI) return;
        s_CalibrationAPI->RecalibrateAllServos();
    }

    bool UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API CheckServoIsMoving()
    {
        return false;
    }

    void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API UpdateServo(char* _jsonCmd)
    {
        if (!s_CalibrationAPI) return;
        nlohmann::json j = nlohmann::json::parse(_jsonCmd);

        s_CalibrationAPI->UpdateServo(j);
    }


    // Test

    void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API GetTestArray(float** a, long* len)
    {
        std::vector<float> tmp = { 1, 23, 4, 5, 4, 3 };
        *len = 6;
        auto size = (*len) * sizeof(float);
        *a = static_cast<float*>(CoTaskMemAlloc(size));
        memcpy(*a, tmp.data(), size);
    }

}
