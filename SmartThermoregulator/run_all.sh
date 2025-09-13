#!/bin/zsh

# Get the absolute path of the current script (project root)
PROJECT_PATH="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

launch_in_terminal() {
    NAME="$1"
    CMD="$2"
    
    echo "🧪 Launching $NAME in a new terminal..."

    osascript -e "
        tell application \"Terminal\"
            do script \"echo '🔧 Running $NAME...'; \
            cd '$PROJECT_PATH/$NAME' || exit 1; \
            $CMD; \
            echo ''; \
            echo '✅ $NAME finished. Press Enter to close...'; \
            read -r\"
        end tell"
}


build_all_projects() {
    echo "Building all projects..."

    NAME0="SmartThermoregulator"

    cd "$PROJECT_PATH/$NAME0"
    echo "Building $NAME0"
    dotnet build || { echo "Build failed for $NAME0"; exit 1; }

}


build_all_projects

# TemperatureRegulator
launch_in_terminal "TemperatureRegulator" "dotnet run"

# CentralHeater
launch_in_terminal "CentralHeater" "dotnet run"

# ReadingDevice
launch_in_terminal "ReadingDevice" "dotnet run"

# ReadingDevice
launch_in_terminal "ReadingDevice" "dotnet run"

# ReadingDevice
launch_in_terminal "ReadingDevice" "dotnet run"

# ReadingDevice
launch_in_terminal "ReadingDevice" "dotnet run"

echo "🚀 All projects launched in new terminal windows."