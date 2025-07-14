#!/bin/bash

# Get the current directory
REPO_DIR=$(pwd)
echo "$REPO_DIR"

# Run the Docker container with the specified environment variables and volume mount
docker run \
	-e IGNORE_GITIGNORED_FILES=true \
	-e LOG_LEVEL=INFO \
	-e DEFAULT_BRANCH=main \
	-e VALIDATE_ALL_CODEBASE=true \
	-e VALIDATE_XML=false \
	-e RUN_LOCAL=true \
	-v "$REPO_DIR:/tmp/lint" -it --rm ghcr.io/super-linter/super-linter:latest

#	-e DEFAULT_BRANCH=origin/develop \
#	-e FILTER_REGEX_EXCLUDE="UA-Nodeset/*" \
#	-e VALIDATE_CHECKOV=false \
#	-e VALIDATE_GITLEAKS=false \
