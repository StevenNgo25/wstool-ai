# Stage 1: Build the Angular app
# Sử dụng một phiên bản Node.js cụ thể (ví dụ: LTS) thay vì 'latest' để đảm bảo tính nhất quán
FROM node:20-alpine AS build
# Hoặc node:18-alpine, tùy theo phiên bản Node bạn muốn

WORKDIR /app

COPY package*.json ./
RUN npm install

COPY . .
# Đảm bảo tên project 'WSTool.client' là chính xác
RUN npm run build --configuration=production 

# Stage 2: Serve the Angular app with Nginx
FROM nginx:alpine

# 1. Xóa bỏ các lệnh chmod không cần thiết và tiềm ẩn rủi ro
# RUN chown -R nginx:nginx /etc/nginx /var/cache/nginx /var/run /var/log/nginx /usr/share/nginx/html \
#   && chmod -R 777 /etc/nginx /var/cache/nginx /var/run /var/log/nginx /usr/share/nginx/html
# Nginx alpine image thường đã được cấu hình để user 'nginx' có thể đọc file tĩnh
# và ghi log/cache vào các thư mục cần thiết mà không cần chmod 777.

# 2. Copy chỉ các file build từ stage trước
COPY --from=build /app/dist/wstool.ui/browser /usr/share/nginx/html

# 3. Copy file cấu hình Nginx cho Angular app (phục vụ qua HTTP)
# File này (nginx/default.conf từ thư mục build context của Dockerfile này)
# nên được cấu hình để Nginx lắng nghe trên cổng 80 HTTP.
COPY ./nginx.conf /etc/nginx/conf.d/default.conf
# Lưu ý: Tên file nginx.conf ở đây là ví dụ, nó phải khớp với tên file cấu hình Nginx
# bạn có trong thư mục build context của Dockerfile này cho Angular app.
# File này KHÁC với file default.conf của Nginx reverse proxy chính.

# 4. Bỏ qua việc copy certs và expose cổng 443 nếu SSL được xử lý bởi reverse proxy chính
# COPY nginx/certs /etc/nginx/certs
# EXPOSE 443

# Chỉ expose cổng 80 cho HTTP
EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]