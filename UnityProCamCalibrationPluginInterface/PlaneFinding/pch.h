#pragma once

#include "targetver.h"

#ifndef WIN32_LEAN_AND_MEAN
#define WIN32_LEAN_AND_MEAN
#endif

#include <windows.h>
#include <intrin.h>

#include <directxmath.h>
#include <directxcollision.h>
#include "MathHelpers.h"

// STL
#include <vector>
#include <map>
#include <queue>
#include <functional>
#include <algorithm>
#include <iostream>
using namespace std;

#ifdef _DEBUG
#include <assert.h>
#define ASSERT assert
#define VERIFY(x) if (!x) std::cout << "VERIFY FAILED: " << __LINE__ << " Add Triangle" << std::endl;
#else
#define ASSERT(x)
#define VERIFY(x) x
#endif