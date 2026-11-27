# SPDX-License-Identifier: Apache-2.0
# Copyright (c) 2025 Aleks Arzer, IFW Hannover. All rights reserved.

# Use official Python base image
FROM python:3.15-slim@sha256:08a4c22c92b79ec821a7ad49768480039ee5b7accac3955228e75366e30abc8e

# Set working directory in the container
WORKDIR /app

# Copy only the files we need
COPY requirements.txt .

# Install dependencies
RUN pip install --no-cache-dir -r requirements.txt

# Copy all contents into the container
COPY . .

# Add a user
RUN useradd -m appuser && chown -R appuser:appuser /app
USER appuser

# Basic healthcheck
HEALTHCHECK --interval=30s --timeout=5s --start-period=10s --retries=3 \
  CMD tail -n 20 /proc/1/fd/1 | grep -q .

# Set the default command to run your app python -u main.py
CMD ["python", "-u", "publish_json_via_mqtt.py"]
