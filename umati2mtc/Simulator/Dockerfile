# Use official Python base image
FROM python:3.11-slim

# Set working directory in the container
WORKDIR /app

# Copy only the files we need
COPY requirements.txt .

# Install dependencies (if you have a requirements.txt)
RUN pip install --no-cache-dir -r requirements.txt

# Copy all contents into the container
COPY . .

# Add a user
RUN useradd -m appuser && chown -R appuser:appuser /app
USER appuser

# Basic healthcheck
HEALTHCHECK --interval=30s --timeout=5s --start-period=10s \
    CMD pgrep -f 'python.*mqtt_message.json' || exit 1

# Set the default command to run your app python -u main.py
CMD ["python", "-u", "publish_json_via_mqtt.py"]
