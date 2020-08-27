#include "stdafx.h"
#include "CalibrationSuiteController.h"



#include "CalibrationSuite/Context.h"
#include "CalibrationSuite/Configuration.h"
#include "CalibrationSuite/CalibrationSuite.h"

#include "StringCast.h"

// TESTING
#include <DirectXMath.h>
#include <d3dcompiler.h>
#include <atlbase.h>

// Get Apps HINSTNACE
EXTERN_C IMAGE_DOS_HEADER __ImageBase;
#define HINST_THISCOMPONENT ((HINSTANCE)&__ImageBase)
#define HMODULE_THISCOMPONENT ((HMODULE)&__ImageBase)

#define CHECK_CONFIG_R(x, r) if (!m_pCalibrationSuiteInterface->IsInitialized())    \
    {                                                                               \
        std::cout << x << std::endl;                                                \
        return r;                                                                   \
    }                                                                                \

#define CHECK_CONFIG(x) CHECK_CONFIG_R(x,)

CalibrationSuiteController::CalibrationSuiteController()
{
   
}


CalibrationSuiteController::~CalibrationSuiteController()
{
}

void CalibrationSuiteController::ProcessDeviceEvent(UnityGfxDeviceEventType type, IUnityInterfaces* interfaces)
{
    // Retrieve executable directory
    switch (type)
    {
    case kUnityGfxDeviceEventInitialize:
    {
        IUnityGraphicsD3D11* d3d = interfaces->Get<IUnityGraphicsD3D11>();
        auto device = d3d->GetDevice();

        if (!m_pContext)
            m_pContext = std::make_shared<cslib::Context>(HINST_THISCOMPONENT, device);

        if (!m_pCalibrationSuiteInterface)
            m_pCalibrationSuiteInterface = &cslib::CalibrationSuite::getInstance();

        break;
    }
    case kUnityGfxDeviceEventShutdown:
        m_pContext.reset();
        break;
    }
}
//
bool CalibrationSuiteController::Configure(const char* _CalibJson)
{
    // Intialize calibration
    std::string dir(_CalibJson);
    return m_pCalibrationSuiteInterface->LoadContextFromPath(string_cast<std::wstring>(dir), m_HINSTANCE, m_pContext);
}

char * CalibrationSuiteController::GetConfigurationStructure()
{
    return nullptr;
}

bool CalibrationSuiteController::IsConfigured()
{
    return m_pCalibrationSuiteInterface != NULL && m_pCalibrationSuiteInterface->IsInitialized();
}

void CalibrationSuiteController::OnUnityStart()
{
    if (!m_pCalibrationSuiteInterface->IsInitialized()) return;

    // Reset Projectors
    auto projectors = m_pCalibrationSuiteInterface->GetConfiguration()->GetProjectors();
    for (auto projector : projectors)
    {
        projector->OnUnityStart();
    }
    
}

std::string CalibrationSuiteController::GetServoReconstructionFile()
{
    CHECK_CONFIG_R("Ensure the Unity Calibration Plugin is Initialized", "NULL");

    return string_cast<std::string>(m_pCalibrationSuiteInterface->GetConfiguration()->GetServoReconstructionFilePath());
}

void CalibrationSuiteController::EnableFullScreen(int _projectorID, bool state)
{
    CHECK_CONFIG("Ensure the Unity Calibration Plugin is Initialized");

    m_pCalibrationSuiteInterface->EnableProjectorsFullscreen(state, _projectorID);
}

void CalibrationSuiteController::IdentifyProjector(int _projectorID, bool state)
{
    CHECK_CONFIG("Ensure the Unity Calibration Plugin is Initialized");

    m_pCalibrationSuiteInterface->DisplayProjectorNames(state, _projectorID);
}

// TESTING
struct SimpleVertex
{
    DirectX::XMFLOAT3 Pos;
    DirectX::XMFLOAT2 Tex;
};

struct CBNeverChanges
{
    DirectX::XMMATRIX mView;
};

struct CBChangeOnResize
{
    DirectX::XMMATRIX mProjection;
};

struct CBChangesEveryFrame
{
    DirectX::XMMATRIX mWorld;
    DirectX::XMFLOAT4 vMeshColor;
};



void CalibrationSuiteController::SetProjectorTexture(int _projectorID, void* _texture, int _w, int _h)
{
    CHECK_CONFIG("Ensure the Unity Calibration Plugin is Initialized");

    assert(_projectorID < m_pCalibrationSuiteInterface->GetConfiguration()->GetProjectors().size());
    auto projector = m_pCalibrationSuiteInterface->GetConfiguration()->GetProjectors()[_projectorID];
    projector->SetUnityTexture((ID3D11Texture2D*)_texture);
}

void CalibrationSuiteController::RenderAllProjectors()
{
    CHECK_CONFIG("Ensure the Unity Calibration Plugin is Initialized");

    auto projectors = m_pCalibrationSuiteInterface->GetConfiguration()->GetProjectors();

    for (auto projector : projectors)
    {
        projector->PresentUnityCameraTarget();
    }
}

void CalibrationSuiteController::CheckProjectorAllCallbacks()
{
    CHECK_CONFIG("Ensure the Unity Calibration Plugin is Initialized");

    if (!m_pCalibrationSuiteInterface) return;
    auto projectors = m_pCalibrationSuiteInterface->GetConfiguration()->GetProjectors();
    for (auto projector : projectors)
    {
        projector->ProcessUnityCallbacks();
    }

}

void CalibrationSuiteController::SetProjectorCallback(int _id, cslib::OnResizeCallback cb)
{
    CHECK_CONFIG("Ensure the Unity Calibration Plugin is Initialized");

    assert(_id < m_pCalibrationSuiteInterface->GetConfiguration()->GetProjectors().size());
    auto projector = m_pCalibrationSuiteInterface->GetConfiguration()->GetProjectors()[_id];

    projector->SetOnResizeCallback(cb);
}

ProjectorDescriptor CalibrationSuiteController::GetProjectorDescriptor(int _id)
{
    CHECK_CONFIG_R("Ensure the Unity Calibration Plugin is Initialized", ProjectorDescriptor() );

    assert(_id < m_pCalibrationSuiteInterface->GetConfiguration()->GetProjectors().size());
    auto projector = m_pCalibrationSuiteInterface->GetConfiguration()->GetProjectors()[_id];

    return {projector->GetCurrentClientRect().right, projector->GetCurrentClientRect().bottom, (int)DXGI_FORMAT_R8G8B8A8_UNORM };
}

ServoRange CalibrationSuiteController::GetServoRange(int _id)
{
    CHECK_CONFIG_R("Ensure the Unity Calibration Plugin is Initialized", ServoRange() );

    // Update servo
    if (_id >= m_pCalibrationSuiteInterface->GetConfiguration()->GetServos().size())
    {
        std::cout << "Servo ID not in range.\n";
        return { -1, -1 };
    }

    auto servo = m_pCalibrationSuiteInterface->GetConfiguration()->GetServos()[_id];
    return { servo->GetRange().x(), servo->GetRange().y() };
}

void CalibrationSuiteController::RecalibrateAllServos()
{
    CHECK_CONFIG("Ensure the Unity Calibration Plugin is Initialized");

    // Update servo
    if (m_pCalibrationSuiteInterface->GetConfiguration()->GetServos().size() <= 0)
    {
        std::cout << "ERROR: No servos created.\n";
        return;
    }

    m_pCalibrationSuiteInterface->GetConfiguration()->GetServos()[0]->Calibrate();
}

void CalibrationSuiteController::UpdateServo(nlohmann::json _j)
{
    CHECK_CONFIG("Ensure the Unity Calibration Plugin is Initialized");

    if (_j.find("degree") == _j.end()) return;

    int servoID = _j["id"];
    int degree = _j["degree"];

    // Update servo
    if (servoID >= m_pCalibrationSuiteInterface->GetConfiguration()->GetServos().size()) 
    {
        std::cout << "Servo ID not in range.\n";
        return;
    }

    // Send command
    auto servo = m_pCalibrationSuiteInterface->GetConfiguration()->GetServos()[servoID];
    servo->Move(degree);
}
