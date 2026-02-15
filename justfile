solution := "src/CronTimer.slnx"

# List available recipes
default:
    @just --list

# Restore NuGet packages
restore:
    dotnet restore {{solution}}

# Build the solution
build *args='':
    dotnet build {{solution}} {{args}}

# Build in Release configuration
build-release:
    dotnet build {{solution}} -c Release

# Pack the NuGet package
pack *args='':
    dotnet pack {{solution}} {{args}}

# Pack in Release configuration
pack-release:
    dotnet pack {{solution}} -c Release

# Clean build artifacts
clean:
    dotnet clean {{solution}}
