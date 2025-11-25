# Contributing to FoLive

Cảm ơn bạn đã quan tâm đến việc đóng góp cho FoLive! 🎉

## Development Setup

1. Fork repository
2. Clone your fork:
   ```bash
   git clone https://github.com/YOUR_USERNAME/FoLive.git
   cd FoLive
   ```

3. Tạo virtual environment:
   ```bash
   python3 -m venv venv
   source venv/bin/activate  # Windows: venv\Scripts\activate
   ```

4. Cài đặt dependencies:
   ```bash
   pip install -r requirements.txt
   ```

5. Cài đặt development dependencies:
   ```bash
   pip install pytest pytest-cov flake8 black
   ```

## Code Style

- Sử dụng **Black** để format code
- Tuân thủ **PEP 8** (với một số exceptions)
- Max line length: 127 characters
- Sử dụng type hints khi có thể

### Format code:
```bash
black .
```

### Check code style:
```bash
flake8 .
black --check .
```

## Testing

Chạy tests trước khi commit:

```bash
# Chạy tất cả tests
pytest tests/ -v

# Với coverage
pytest tests/ -v --cov=. --cov-report=html
```

## Commit Messages

Sử dụng conventional commits:

- `feat:` - Tính năng mới
- `fix:` - Sửa lỗi
- `docs:` - Cập nhật documentation
- `test:` - Thêm/sửa tests
- `refactor:` - Refactor code
- `chore:` - Các thay đổi khác

Ví dụ:
```
feat: Add support for RTSP streams
fix: Fix YouTube playlist handling
docs: Update installation instructions
```

## Pull Request Process

1. Tạo branch mới từ `main`:
   ```bash
   git checkout -b feature/your-feature-name
   ```

2. Commit changes:
   ```bash
   git add .
   git commit -m "feat: Add new feature"
   ```

3. Push và tạo PR:
   ```bash
   git push origin feature/your-feature-name
   ```

4. Đảm bảo:
   - ✅ Tests pass
   - ✅ Code được format
   - ✅ Không có linter errors
   - ✅ Có documentation nếu cần

## Testing Checklist

Trước khi submit PR, đảm bảo:

- [ ] Code được format với Black
- [ ] Không có linter errors
- [ ] Tests pass (nếu có)
- [ ] Đã test trên local
- [ ] Documentation được cập nhật (nếu cần)

## Questions?

Nếu có câu hỏi, tạo issue hoặc liên hệ maintainers.

Cảm ơn bạn đã đóng góp! 🙏


