#pragma once

#include <nlohmann/json.hpp>

struct ProjectorDescriptor {
    int width, height, DXGI_FROMATE_TYPE;
};

struct ServoRange
{
    int min, max;
};


namespace cslib
{
    class CalibrationSuite;
    class Configuration;
    class Context;
    typedef void(__stdcall * OnResizeCallback)(int, int, int); // width, height, type

}

class CalibrationSuiteController
{
private:
    cslib::CalibrationSuite* m_pCalibrationSuiteInterface = NULL;
    std::shared_ptr<cslib::Context> m_pContext;
    HINSTANCE m_HINSTANCE;


public:
    CalibrationSuiteController();
    ~CalibrationSuiteController();

    // Unity
    void ProcessDeviceEvent(UnityGfxDeviceEventType type, IUnityInterfaces* interfaces);

    // Configure calibration
    bool Configure(const char* _CalibJson);
    char* GetConfigurationStructure();
    bool IsConfigured();
    void OnUnityStart();
    std::string GetServoReconstructionFile();
    

    // Projector Interface
    void EnableFullScreen(int _projectorID = -1, bool state = true);
    void IdentifyProjector(int _projectorID = -1, bool state = true);
    void SetProjectorTexture(int _projectorID, void* _texture, int _w, int _h);
    void RenderAllProjectors();
    void CheckProjectorAllCallbacks();
    void SetProjectorCallback(int _id, cslib::OnResizeCallback cb);
    ProjectorDescriptor GetProjectorDescriptor(int _id);

    // Servo Interface
    ServoRange GetServoRange(int _id);
    void RecalibrateAllServos();
    void UpdateServo(nlohmann::json _j);
};
