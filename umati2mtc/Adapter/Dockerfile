# SPDX-License-Identifier: Apache-2.0
# Copyright (c) 2025 Aleks Arzer, Institut für Fertigungstechnik und Werkzeugmaschinen, Leibniz Universität Hannover. All rights reserved.

# Use official Python base image
FROM python:3.11-slim

# Set working directory in the container
WORKDIR /app

# Copy only the files we need
COPY requirements.txt .

# Install dependencies
RUN pip install --no-cache-dir -r requirements.txt

# Copy all contents into the container
COPY . .

# Optional: expose port if it's a web app
EXPOSE 7878

# Add a user
RUN useradd -m appuser
USER appuser

# Basic healthcheck: check if the app is running by looking for recent output in the logs
HEALTHCHECK --interval=30s --timeout=5s --start-period=10s --retries=3 \
  CMD tail -n 20 /proc/1/fd/1 | grep -q .

# Set the default command to run your app
CMD ["python", "-u", "main.py"]
