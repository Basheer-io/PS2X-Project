cmake_minimum_required(VERSION 3.20)
project(PS2HomeNative LANGUAGES CXX)

set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)
set(CMAKE_CXX_EXTENSIONS OFF)

add_executable(ps2home-diag
    src/main.cpp
)

if(WIN32)
    target_compile_definitions(ps2home-diag PRIVATE PLATFORM_WINDOWS)
else()
    target_compile_definitions(ps2home-diag PRIVATE PLATFORM_LINUX)
    target_link_libraries(ps2home-diag PRIVATE ${CMAKE_DL_LIBS})
endif()

# Installation / Output directory
set_target_properties(ps2home-diag PROPERTIES
    OUTPUT_NAME "ps2home-diag"
    RUNTIME_OUTPUT_DIRECTORY "${CMAKE_BINARY_DIR}/bin"
)
