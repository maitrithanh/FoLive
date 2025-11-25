.PHONY: help install test lint format clean run docker-build docker-run

help:
	@echo "FoLive - Makefile Commands"
	@echo ""
	@echo "  make install    - Install dependencies"
	@echo "  make test       - Run tests"
	@echo "  make lint       - Run linter"
	@echo "  make format     - Format code with black"
	@echo "  make clean      - Clean temporary files"
	@echo "  make run        - Run application"
	@echo "  make docker-build - Build Docker image"
	@echo "  make docker-run   - Run Docker container"

install:
	pip install -r requirements.txt

test:
	pytest tests/ -v --cov=. --cov-report=html

lint:
	flake8 .
	black --check .

format:
	black .

clean:
	rm -rf __pycache__ *.pyc .pytest_cache .coverage htmlcov dist build *.egg-info
	rm -rf temp/* output/*

run:
	python run.py

docker-build:
	docker build -t folive:latest .

docker-run:
	docker run -p 5000:5000 folive:latest


