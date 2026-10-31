from http.server import ThreadingHTTPServer, SimpleHTTPRequestHandler
from pathlib import Path
import re
import functools

class ReviewHandler(SimpleHTTPRequestHandler):
    def send_head(self):
        self.remaining = None
        path=Path(self.translate_path(self.path))
        requested=self.headers.get('Range')
        if not requested or not path.is_file():
            return super().send_head()
        match=re.fullmatch(r'bytes=(\d+)-(\d*)',requested)
        size=path.stat().st_size
        if not match:
            self.send_error(416);return None
        start=int(match[1]);end=min(int(match[2]) if match[2] else size-1,size-1)
        if start>end:
            self.send_error(416);return None
        stream=path.open('rb');stream.seek(start)
        self.remaining=end-start+1
        self.send_response(206)
        self.send_header('Content-Type',self.guess_type(str(path)))
        self.send_header('Content-Length',str(self.remaining))
        self.send_header('Content-Range',f'bytes {start}-{end}/{size}')
        self.send_header('Accept-Ranges','bytes')
        self.end_headers()
        return stream
    def end_headers(self):
        self.send_header('Accept-Ranges','bytes')
        super().end_headers()
    def copyfile(self,source,outputfile):
        if self.remaining is None:return super().copyfile(source,outputfile)
        remaining=self.remaining
        while remaining:
            chunk=source.read(min(65536,remaining))
            if not chunk:break
            outputfile.write(chunk);remaining-=len(chunk)

ThreadingHTTPServer(('127.0.0.1',8772),functools.partial(ReviewHandler,directory=str(Path(__file__).parent))).serve_forever()
