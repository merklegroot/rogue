#!/bin/bash

function build_windows() {
    # Windows outputs here:
    # Rogue/bin/Release/net8.0/win-x64/publish

    # Windows
    dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true
}

function build_linux() {
    # Linux outputs here:
    # Rogue/bin/Release/net8.0/linux-x64/publish
    dotnet publish -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true
    echo ./Rouge > Rogue/bin/Release/net8.0/linux-x64/publish/run.sh
}

build_linux
