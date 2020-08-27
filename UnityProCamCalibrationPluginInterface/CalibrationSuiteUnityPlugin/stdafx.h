// stdafx.h : include file for standard system include files,
// or project specific include files that are used frequently, but
// are changed infrequently
//

#pragma once

#include "targetver.h"

#define WIN32_LEAN_AND_MEAN             // Exclude rarely-used stuff from Windows headers
// Windows Header Files
#include <windows.h>



// reference additional headers your program requires here
#include <iostream>
#include <thread>
#include <memory>

#include <wincodec.h>
#include <d3d11_4.h>
#include <dxgi1_6.h>
#include <d2d1.h>
#include <dxgidebug.h>
#include <DXProgrammableCapture.h>
#include <atlbase.h>
#include <mutex>
#include <dwrite.h>


// Unity
#include "IUnityGraphics.h"
#include "IUnityGraphicsD3D11.h"

