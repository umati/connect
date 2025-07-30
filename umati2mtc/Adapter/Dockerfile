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

# Optional: expose port if it's a web app
EXPOSE 7878

# Set the default command to run your app
CMD ["python", "main.py"]
