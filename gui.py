"""
FoLive - Desktop GUI Application for Windows
"""
import tkinter as tk
from tkinter import ttk, messagebox, filedialog, scrolledtext
import threading
import json
import os
from datetime import datetime
from stream_manager import StreamManager, StreamStatus
from source_handlers import SourceHandlerFactory
from ffmpeg_handler import FFmpegHandler
from config import HOST, PORT

class FoLiveGUI:
    """Main GUI Application"""
    
    def __init__(self, root):
        self.root = root
        self.root.title("FoLive - Livestream 24/7 Manager")
        self.root.geometry("1200x800")
        self.root.minsize(1000, 600)
        
        # Initialize managers
        self.stream_manager = StreamManager()
        self.ffmpeg_handler = FFmpegHandler()
        
        # Variables
        self.streams_data = {}
        
        # Setup UI
        self.setup_ui()
        
        # Start auto-refresh
        self.refresh_streams()
        self.root.after(3000, self.auto_refresh)
    
    def setup_ui(self):
        """Setup user interface"""
        # Main container
        main_frame = ttk.Frame(self.root, padding="10")
        main_frame.grid(row=0, column=0, sticky=(tk.W, tk.E, tk.N, tk.S))
        
        self.root.columnconfigure(0, weight=1)
        self.root.rowconfigure(0, weight=1)
        main_frame.columnconfigure(1, weight=1)
        main_frame.rowconfigure(1, weight=1)
        
        # Left panel - Create stream
        left_panel = ttk.LabelFrame(main_frame, text="Tạo Stream Mới", padding="10")
        left_panel.grid(row=0, column=0, rowspan=2, sticky=(tk.W, tk.E, tk.N, tk.S), padx=(0, 10))
        left_panel.columnconfigure(0, weight=1)
        
        # Stream ID
        ttk.Label(left_panel, text="Stream ID:").grid(row=0, column=0, sticky=tk.W, pady=5)
        self.stream_id_var = tk.StringVar()
        ttk.Entry(left_panel, textvariable=self.stream_id_var, width=25).grid(row=0, column=1, pady=5, sticky=(tk.W, tk.E))
        
        # Source type
        ttk.Label(left_panel, text="Loại nguồn:").grid(row=1, column=0, sticky=tk.W, pady=5)
        self.source_type_var = tk.StringVar(value="file")
        source_type_combo = ttk.Combobox(left_panel, textvariable=self.source_type_var, 
                                        values=["file", "youtube", "playlist", "screen"], 
                                        state="readonly", width=22)
        source_type_combo.grid(row=1, column=1, pady=5, sticky=(tk.W, tk.E))
        source_type_combo.bind("<<ComboboxSelected>>", self.on_source_type_change)
        
        # Source
        ttk.Label(left_panel, text="Nguồn:").grid(row=2, column=0, sticky=tk.W, pady=5)
        source_frame = ttk.Frame(left_panel)
        source_frame.grid(row=2, column=1, pady=5, sticky=(tk.W, tk.E))
        source_frame.columnconfigure(0, weight=1)
        
        self.source_var = tk.StringVar()
        self.source_entry = ttk.Entry(source_frame, textvariable=self.source_var, width=20)
        self.source_entry.grid(row=0, column=0, sticky=(tk.W, tk.E), padx=(0, 5))
        
        self.browse_btn = ttk.Button(source_frame, text="Browse", command=self.browse_file)
        self.browse_btn.grid(row=0, column=1)
        
        # Stream URL
        ttk.Label(left_panel, text="Stream URL (RTMP):").grid(row=3, column=0, sticky=tk.W, pady=5)
        self.stream_url_var = tk.StringVar(value="rtmp://a.rtmp.youtube.com/live2")
        ttk.Entry(left_panel, textvariable=self.stream_url_var, width=25).grid(row=3, column=1, pady=5, sticky=(tk.W, tk.E))
        
        # Stream Key
        ttk.Label(left_panel, text="Stream Key:").grid(row=4, column=0, sticky=tk.W, pady=5)
        self.stream_key_var = tk.StringVar()
        ttk.Entry(left_panel, textvariable=self.stream_key_var, width=25, show="*").grid(row=4, column=1, pady=5, sticky=(tk.W, tk.E))
        
        # Advanced config
        config_frame = ttk.LabelFrame(left_panel, text="Cấu hình nâng cao", padding="5")
        config_frame.grid(row=5, column=0, columnspan=2, pady=10, sticky=(tk.W, tk.E))
        config_frame.columnconfigure(1, weight=1)
        
        # Bitrate
        ttk.Label(config_frame, text="Bitrate (k):").grid(row=0, column=0, sticky=tk.W, pady=2)
        self.bitrate_var = tk.StringVar(value="2500")
        ttk.Entry(config_frame, textvariable=self.bitrate_var, width=15).grid(row=0, column=1, pady=2, sticky=tk.W)
        
        # Speed
        ttk.Label(config_frame, text="Tốc độ:").grid(row=1, column=0, sticky=tk.W, pady=2)
        self.speed_var = tk.StringVar(value="1.0")
        ttk.Entry(config_frame, textvariable=self.speed_var, width=15).grid(row=1, column=1, pady=2, sticky=tk.W)
        
        # Volume
        ttk.Label(config_frame, text="Âm lượng:").grid(row=2, column=0, sticky=tk.W, pady=2)
        self.volume_var = tk.StringVar(value="1.0")
        ttk.Entry(config_frame, textvariable=self.volume_var, width=15).grid(row=2, column=1, pady=2, sticky=tk.W)
        
        # Brightness
        ttk.Label(config_frame, text="Độ sáng:").grid(row=3, column=0, sticky=tk.W, pady=2)
        self.brightness_var = tk.StringVar(value="0")
        ttk.Entry(config_frame, textvariable=self.brightness_var, width=15).grid(row=3, column=1, pady=2, sticky=tk.W)
        
        # Text overlay
        ttk.Label(config_frame, text="Chèn chữ:").grid(row=4, column=0, sticky=tk.W, pady=2)
        self.text_var = tk.StringVar()
        ttk.Entry(config_frame, textvariable=self.text_var, width=15).grid(row=4, column=1, pady=2, sticky=(tk.W, tk.E))
        
        # Loop
        self.loop_var = tk.BooleanVar(value=True)
        ttk.Checkbutton(config_frame, text="Lặp lại vô hạn (24/7)", variable=self.loop_var).grid(row=5, column=0, columnspan=2, pady=5, sticky=tk.W)
        
        # Buttons
        btn_frame = ttk.Frame(left_panel)
        btn_frame.grid(row=6, column=0, columnspan=2, pady=10)
        
        ttk.Button(btn_frame, text="Tạo Stream", command=self.create_stream).pack(side=tk.LEFT, padx=5)
        ttk.Button(btn_frame, text="Kiểm tra nguồn", command=self.validate_source).pack(side=tk.LEFT, padx=5)
        
        # Right panel - Streams list
        right_panel = ttk.LabelFrame(main_frame, text="Danh sách Streams", padding="10")
        right_panel.grid(row=0, column=1, sticky=(tk.W, tk.E, tk.N, tk.S))
        right_panel.columnconfigure(0, weight=1)
        right_panel.rowconfigure(1, weight=1)
        
        # Stats
        stats_frame = ttk.Frame(right_panel)
        stats_frame.grid(row=0, column=0, sticky=(tk.W, tk.E), pady=(0, 10))
        
        self.stats_label = ttk.Label(stats_frame, text="Tổng: 0 | Đang chạy: 0", font=("Arial", 10, "bold"))
        self.stats_label.pack()
        
        # Streams listbox with scrollbar
        list_frame = ttk.Frame(right_panel)
        list_frame.grid(row=1, column=0, sticky=(tk.W, tk.E, tk.N, tk.S))
        list_frame.columnconfigure(0, weight=1)
        list_frame.rowconfigure(0, weight=1)
        
        scrollbar = ttk.Scrollbar(list_frame)
        scrollbar.grid(row=0, column=1, sticky=(tk.N, tk.S))
        
        self.streams_listbox = tk.Listbox(list_frame, yscrollcommand=scrollbar.set, height=15)
        self.streams_listbox.grid(row=0, column=0, sticky=(tk.W, tk.E, tk.N, tk.S))
        scrollbar.config(command=self.streams_listbox.yview)
        
        # Stream actions
        action_frame = ttk.Frame(right_panel)
        action_frame.grid(row=2, column=0, pady=10)
        
        ttk.Button(action_frame, text="Bắt đầu", command=self.start_selected_stream).pack(side=tk.LEFT, padx=5)
        ttk.Button(action_frame, text="Dừng", command=self.stop_selected_stream).pack(side=tk.LEFT, padx=5)
        ttk.Button(action_frame, text="Xóa", command=self.delete_selected_stream).pack(side=tk.LEFT, padx=5)
        ttk.Button(action_frame, text="Làm mới", command=self.refresh_streams).pack(side=tk.LEFT, padx=5)
        
        # Log area
        log_frame = ttk.LabelFrame(main_frame, text="Logs", padding="5")
        log_frame.grid(row=1, column=1, sticky=(tk.W, tk.E, tk.N, tk.S), pady=(10, 0))
        log_frame.columnconfigure(0, weight=1)
        log_frame.rowconfigure(0, weight=1)
        
        self.log_text = scrolledtext.ScrolledText(log_frame, height=10, wrap=tk.WORD)
        self.log_text.grid(row=0, column=0, sticky=(tk.W, tk.E, tk.N, tk.S))
        self.log_text.config(state=tk.DISABLED)
    
    def on_source_type_change(self, event=None):
        """Handle source type change"""
        source_type = self.source_type_var.get()
        if source_type == "file":
            self.browse_btn.config(state=tk.NORMAL)
            self.source_entry.config(state=tk.NORMAL)
        elif source_type == "screen":
            self.browse_btn.config(state=tk.DISABLED)
            self.source_entry.config(state=tk.DISABLED)
            self.source_var.set("1:0")
        else:
            self.browse_btn.config(state=tk.DISABLED)
            self.source_entry.config(state=tk.NORMAL)
    
    def browse_file(self):
        """Browse for video file"""
        filename = filedialog.askopenfilename(
            title="Chọn file video",
            filetypes=[
                ("Video files", "*.mp4 *.mov *.mkv *.avi *.flv *.webm"),
                ("All files", "*.*")
            ]
        )
        if filename:
            self.source_var.set(filename)
    
    def log(self, message):
        """Add log message"""
        self.log_text.config(state=tk.NORMAL)
        timestamp = datetime.now().strftime("%H:%M:%S")
        self.log_text.insert(tk.END, f"[{timestamp}] {message}\n")
        self.log_text.see(tk.END)
        self.log_text.config(state=tk.DISABLED)
    
    def validate_source(self):
        """Validate source"""
        source_type = self.source_type_var.get()
        source = self.source_var.get()
        
        if not source:
            messagebox.showwarning("Cảnh báo", "Vui lòng nhập nguồn")
            return
        
        try:
            handler = SourceHandlerFactory.get_handler(source_type)
            if handler.validate(source):
                messagebox.showinfo("Thành công", "Nguồn hợp lệ!")
                self.log(f"Nguồn hợp lệ: {source_type} - {source}")
            else:
                messagebox.showerror("Lỗi", "Nguồn không hợp lệ")
        except Exception as e:
            messagebox.showerror("Lỗi", f"Lỗi: {str(e)}")
            self.log(f"Lỗi validate: {str(e)}")
    
    def create_stream(self):
        """Create new stream"""
        stream_id = self.stream_id_var.get()
        source_type = self.source_type_var.get()
        source = self.source_var.get()
        stream_url = self.stream_url_var.get()
        stream_key = self.stream_key_var.get()
        
        # Validation
        if not all([stream_id, source, stream_url, stream_key]):
            messagebox.showerror("Lỗi", "Vui lòng điền đầy đủ thông tin")
            return
        
        try:
            # Validate source
            handler = SourceHandlerFactory.get_handler(source_type)
            if not handler.validate(source):
                messagebox.showerror("Lỗi", "Nguồn không hợp lệ")
                return
            
            # Get stream source
            stream_source = handler.get_stream_url(source)
            if not stream_source:
                messagebox.showerror("Lỗi", "Không thể lấy nguồn stream")
                return
            
            # Build config
            config = {
                'bitrate': f"{self.bitrate_var.get()}k",
                'speed': float(self.speed_var.get()),
                'volume': float(self.volume_var.get()),
                'brightness': int(self.brightness_var.get()),
                'loop': self.loop_var.get(),
                'text': self.text_var.get() if self.text_var.get() else None
            }
            
            # Create stream
            stream = self.stream_manager.create_stream(
                stream_id, stream_source, source_type,
                stream_key, stream_url, config
            )
            
            messagebox.showinfo("Thành công", f"Đã tạo stream: {stream_id}")
            self.log(f"Đã tạo stream: {stream_id}")
            
            # Clear form
            self.stream_id_var.set("")
            self.source_var.set("")
            
            # Refresh list
            self.refresh_streams()
            
        except ValueError as e:
            messagebox.showerror("Lỗi", f"Stream ID đã tồn tại: {str(e)}")
        except Exception as e:
            messagebox.showerror("Lỗi", f"Lỗi: {str(e)}")
            self.log(f"Lỗi tạo stream: {str(e)}")
    
    def refresh_streams(self):
        """Refresh streams list"""
        streams = self.stream_manager.get_all_streams()
        self.streams_data = {s['stream_id']: s for s in streams}
        
        # Update listbox
        self.streams_listbox.delete(0, tk.END)
        for stream in streams:
            status_icon = {
                'running': '▶',
                'idle': '⏸',
                'error': '❌',
                'stopped': '⏹'
            }.get(stream['status'], '?')
            
            display = f"{status_icon} {stream['stream_id']} - {stream['source_type']} ({stream['status']})"
            self.streams_listbox.insert(tk.END, display)
        
        # Update stats
        total = len(streams)
        running = len([s for s in streams if s['status'] == 'running'])
        self.stats_label.config(text=f"Tổng: {total} | Đang chạy: {running}")
    
    def get_selected_stream_id(self):
        """Get selected stream ID"""
        selection = self.streams_listbox.curselection()
        if not selection:
            return None
        
        index = selection[0]
        stream_id = list(self.streams_data.keys())[index]
        return stream_id
    
    def start_selected_stream(self):
        """Start selected stream"""
        stream_id = self.get_selected_stream_id()
        if not stream_id:
            messagebox.showwarning("Cảnh báo", "Vui lòng chọn stream")
            return
        
        try:
            if self.stream_manager.start_stream(stream_id):
                messagebox.showinfo("Thành công", f"Đã bắt đầu stream: {stream_id}")
                self.log(f"Đã bắt đầu stream: {stream_id}")
                self.refresh_streams()
            else:
                messagebox.showerror("Lỗi", "Không thể bắt đầu stream")
        except Exception as e:
            messagebox.showerror("Lỗi", f"Lỗi: {str(e)}")
            self.log(f"Lỗi start stream: {str(e)}")
    
    def stop_selected_stream(self):
        """Stop selected stream"""
        stream_id = self.get_selected_stream_id()
        if not stream_id:
            messagebox.showwarning("Cảnh báo", "Vui lòng chọn stream")
            return
        
        try:
            if self.stream_manager.stop_stream(stream_id):
                messagebox.showinfo("Thành công", f"Đã dừng stream: {stream_id}")
                self.log(f"Đã dừng stream: {stream_id}")
                self.refresh_streams()
            else:
                messagebox.showerror("Lỗi", "Không thể dừng stream")
        except Exception as e:
            messagebox.showerror("Lỗi", f"Lỗi: {str(e)}")
            self.log(f"Lỗi stop stream: {str(e)}")
    
    def delete_selected_stream(self):
        """Delete selected stream"""
        stream_id = self.get_selected_stream_id()
        if not stream_id:
            messagebox.showwarning("Cảnh báo", "Vui lòng chọn stream")
            return
        
        if not messagebox.askyesno("Xác nhận", f"Bạn có chắc muốn xóa stream: {stream_id}?"):
            return
        
        try:
            if self.stream_manager.remove_stream(stream_id):
                messagebox.showinfo("Thành công", f"Đã xóa stream: {stream_id}")
                self.log(f"Đã xóa stream: {stream_id}")
                self.refresh_streams()
            else:
                messagebox.showerror("Lỗi", "Không thể xóa stream")
        except Exception as e:
            messagebox.showerror("Lỗi", f"Lỗi: {str(e)}")
            self.log(f"Lỗi delete stream: {str(e)}")
    
    def auto_refresh(self):
        """Auto refresh streams"""
        self.refresh_streams()
        self.root.after(3000, self.auto_refresh)

def main():
    """Main function"""
    root = tk.Tk()
    app = FoLiveGUI(root)
    root.mainloop()

if __name__ == '__main__':
    main()

