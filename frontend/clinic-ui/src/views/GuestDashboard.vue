<template>
  <div class="guest-dashboard">
    <!-- Hero -->
    <section class="hero">
      <div class="hero-background">
        <div class="shape shape-1"></div>
        <div class="shape shape-2"></div>
        <div class="shape shape-3"></div>
        <div class="hero-grid-pattern" aria-hidden="true"></div>
      </div>
      <div class="container hero-inner">
        <div class="hero-copy">
          <p class="hero-eyebrow">Chăm sóc sức khỏe chủ động</p>
          <h1 class="hero-title">Phòng khám đồng hành cùng bạn</h1>
          <p class="hero-subtitle">
            Đặt lịch trực tuyến trong vài phút, xác thực OTP qua email và nhận mã khám để tra cứu mọi lúc.
          </p>
          <div class="hero-actions">
            <button type="button" class="btn btn-hero-primary" @click="scrollToBooking">
              <i class="bi bi-calendar-check me-2"></i>Đặt lịch hẹn
            </button>
            <button type="button" class="btn btn-hero-ghost" @click="scrollToSearch">
              <i class="bi bi-search me-2"></i>Tra cứu lịch khám
            </button>
          </div>
          <ul class="hero-trust" aria-label="Cam kết">
            <li><i class="bi bi-shield-check" aria-hidden="true"></i><span>OTP bảo vệ email</span></li>
            <li><i class="bi bi-clock-history" aria-hidden="true"></i><span>Giờ làm việc 07:00–22:00</span></li>
            <li><i class="bi bi-hospital" aria-hidden="true"></i><span>Đa khoa &amp; chuyên khoa</span></li>
          </ul>
        </div>
        <aside class="hero-aside" aria-label="Thông tin nhanh">
          <div class="hero-stat-card">
            <div class="hero-stat-row">
              <div class="hero-stat">
                <span class="hero-stat-value">5′</span>
                <span class="hero-stat-label">đặt lịch trung bình</span>
              </div>
              <div class="hero-stat">
                <span class="hero-stat-value">24/7</span>
                <span class="hero-stat-label">hỗ trợ tra cứu online</span>
              </div>
            </div>
            <div class="hero-stat-divider"></div>
            <p class="hero-stat-note">
              Sau khi đặt lịch, bạn nhận mã khám qua email — dùng mã và SĐT để kiểm tra hoặc huỷ lịch khi cần.
            </p>
            <button type="button" class="btn btn-hero-card-cta w-100" @click="scrollToBooking">
              Bắt đầu đặt lịch
              <i class="bi bi-arrow-right ms-2"></i>
            </button>
          </div>
        </aside>
      </div>
    </section>

    <!-- Services -->
    <section id="services" class="services-section">
      <div class="container">
        <p class="section-eyebrow">Dịch vụ</p>
        <h2 class="section-title">Dịch vụ nổi bật</h2>
        <p class="section-subtitle">
          Các chuyên khoa chính và dịch vụ hỗ trợ thường gặp, giúp bạn đặt lịch đúng nhu cầu ngay từ đầu.
        </p>
        <div class="services-grid">
          <article v-for="s in services" :key="s.id" class="service-card">
            <div :class="['service-icon', `icon-${s.color}`]">
              <i :class="s.icon" aria-hidden="true"></i>
            </div>
            <h3>{{ s.name }}</h3>
            <p>{{ s.description }}</p>
          </article>
        </div>
      </div>
    </section>

    <!-- Booking -->
    <section id="booking" class="booking-section">
      <div class="container">
        <p class="section-eyebrow section-eyebrow--on-soft">Đặt lịch</p>
        <h2 class="section-title">Đặt lịch khám</h2>
        <p class="section-subtitle">Nhập thông tin, xác thực email bằng OTP rồi gửi yêu cầu — chúng tôi sẽ xác nhận qua email.</p>

        <div class="row g-4 align-items-start">
          <div class="col-lg-7">
            <!-- Success -->
            <div v-if="bookingSuccess" class="alert alert-success" ref="bookingSuccessRef">
              <i class="bi bi-check-circle me-2"></i>
              <div>
                <strong>Lịch khám đã được đặt thành công!</strong>
                <p class="mt-2">
                  <strong>Mã khám:</strong> <span class="appointment-code">{{ bookingResponse.appointmentCode }}</span>
                </p>
                <p><strong>Trạng thái:</strong> <span class="badge badge-info">{{ bookingResponse.status }}</span></p>
                <p>
                  <strong>Ngày & Giờ:</strong>
                  {{ formatDateTime(bookingResponse.appointmentDate, bookingResponse.appointmentTime) }}
                </p>
              </div>
            </div>

            <!-- Error -->
            <div v-if="bookingError" class="alert alert-danger">
              <i class="bi bi-exclamation-circle me-2"></i>
              <div>
                <strong>Lỗi</strong>
                <p class="mt-2">{{ bookingError }}</p>
              </div>
            </div>

            <!-- Booking Form -->
            <form v-if="!bookingSuccess" class="booking-form" @submit.prevent="submitBooking">
              <!-- Returning patient -->
              <div class="returning-box">
                <div class="d-flex justify-content-between align-items-start gap-2 flex-wrap">
                  <div>
                    <button type="button" class="btn btn-link p-0 text-decoration-none" @click="showReturningLookup = !showReturningLookup">
                      <span class="fw-bold text-body">Bệnh nhân cũ?</span>
                      <i class="bi ms-1" :class="showReturningLookup ? 'bi-chevron-up' : 'bi-chevron-down'"></i>
                    </button>
                    <div v-if="!showReturningLookup" class="text-muted small">
                      Nhập SĐT hoặc email rồi bấm “Điền thông tin cũ”.
                    </div>
                  </div>
                  <span v-if="isReturning" class="badge bg-success-subtle text-success">Đã điền từ hồ sơ trước</span>
                </div>

                <div v-show="showReturningLookup" class="mt-3">
                  <div class="form-row">
                    <div class="form-group">
                      <input
                        v-model="lookupPhone"
                        type="tel"
                        class="form-input"
                        placeholder="Số điện thoại"
                        inputmode="numeric"
                        maxlength="11"
                        @input="lookupPhone = normalizePhoneInput(lookupPhone)"
                      />
                    </div>
                    <div class="form-group">
                      <input v-model="lookupEmail" type="email" class="form-input" placeholder="Email" />
                    </div>
                    <div class="form-group">
                      <button type="button" class="btn btn-outline-primary w-100" :disabled="lookupLoading" @click="lookupPatient">
                        <span v-if="lookupLoading" class="spinner-border spinner-border-sm me-1"></span>
                        Điền thông tin cũ
                      </button>
                    </div>
                    <div class="form-group" v-if="isReturning">
                      <button type="button" class="btn btn-secondary w-100" @click="clearPrefill">Chỉnh sửa</button>
                    </div>
                  </div>
                  <div v-if="lookupError" class="alert alert-warning py-2 my-2 mb-0">{{ lookupError }}</div>
                </div>
              </div>

              <!-- Patient info -->
              <div class="form-row">
                <div class="form-group">
                  <label class="form-label">Họ và tên *</label>
                  <input v-model="bookingForm.fullName" type="text" class="form-input" :readonly="isReturning" required />
                  <span v-if="bookingErrors.fullName" class="form-error">{{ bookingErrors.fullName }}</span>
                </div>
                <div class="form-group">
                  <label class="form-label">Ngày sinh *</label>
                  <input v-model="bookingForm.dateOfBirth" type="date" class="form-input" :readonly="isReturning" required />
                  <span v-if="bookingErrors.dateOfBirth" class="form-error">{{ bookingErrors.dateOfBirth }}</span>
                </div>
              </div>

              <div class="form-row">
                <div class="form-group">
                  <label class="form-label">Giới tính *</label>
                  <select v-model="bookingForm.gender" class="form-select" :disabled="isReturning" required>
                    <option value="">Chọn giới tính</option>
                    <option value="1">Nam</option>
                    <option value="2">Nữ</option>
                  </select>
                  <span v-if="bookingErrors.gender" class="form-error">{{ bookingErrors.gender }}</span>
                </div>
                <div class="form-group">
                  <label class="form-label">Điện thoại *</label>
                  <input
                    v-model="bookingForm.phone"
                    type="tel"
                    class="form-input"
                    :readonly="isReturning"
                    required
                    inputmode="numeric"
                    maxlength="11"
                    @input="bookingForm.phone = normalizePhoneInput(bookingForm.phone)"
                  />
                  <span v-if="bookingErrors.phone" class="form-error">{{ bookingErrors.phone }}</span>
                </div>
              </div>

              <div class="form-row">
                <div class="form-group">
                  <label class="form-label">Email *</label>
                  <input v-model="bookingForm.email" type="email" class="form-input" required />
                  <span v-if="bookingErrors.email" class="form-error">{{ bookingErrors.email }}</span>
                </div>
                <div class="form-group">
                  <label class="form-label">Địa chỉ *</label>
                  <input v-model="bookingForm.address" type="text" class="form-input" :readonly="isReturning" required />
                  <span v-if="bookingErrors.address" class="form-error">{{ bookingErrors.address }}</span>
                </div>
              </div>
              <div class="form-row">
                <div class="form-group">
                  <label class="form-label">Số CCCD/Passport</label>
                  <input 
                    v-model="bookingForm.CitizenId" 
                    type="tel" 
                    inputmode="numeric"
                    pattern="[0-9]*"
                    class="form-input" 
                    :readonly="isReturning"
                    maxlength="12"
                    placeholder="Nhập số CCCD"
                  />
                </div>
                <div class="form-group">
                  <label class="form-label">Mã số BHYT (nếu có)</label>
                  <input 
                    v-model="bookingForm.insuranceNumber" 
                    type="text" 
                    class="form-input" 
                    :readonly="isReturning"
                    placeholder="Nhập mã BHYT"
                  />
                </div>
              </div>
              <!-- OTP -->
              <div class="form-row align-items-end otp-row">
                <div class="form-group flex-grow-1">
                  <label class="form-label">Mã OTP</label>
                  <div class="d-flex flex-wrap gap-2">
                    <input v-model="otpCode" type="text" maxlength="6" class="form-input flex-grow-1" placeholder="Nhập mã OTP" />
                    <button type="button" class="btn btn-outline-success" :disabled="otpVerifying || otpVerified" @click="verifyOtp">
                      <span v-if="otpVerifying" class="spinner-border spinner-border-sm me-1"></span>
                      {{ otpVerified ? 'Đã xác thực' : 'Xác thực OTP' }}
                    </button>
                  </div>
                  <div class="text-danger small mt-1" v-if="otpError">{{ otpError }}</div>
                  <div class="text-success small" v-else-if="otpVerified">Email đã được xác thực.</div>
                  <div class="text-muted small" v-else>Bạn cần xác thực email trước khi đặt lịch.</div>
                </div>
                <div class="form-group" style="min-width: 200px;">
                  <label class="form-label">&nbsp;</label>
                  <button type="button" class="btn btn-primary w-100" :disabled="otpSending || countdown > 0" @click="sendOtp">
                    <span v-if="otpSending" class="spinner-border spinner-border-sm me-1"></span>
                    {{ countdown > 0 ? `Gửi lại (${countdown}s)` : 'Gửi mã OTP' }}
                  </button>
                  <div class="text-muted small mt-1" v-if="countdown > 0">Có thể gửi lại sau {{ countdown }}s</div>
                </div>
              </div>

              <!-- Appointment info -->
              <div class="form-row">
                <div class="form-group">
                  <label class="form-label">Ngày khám *</label>
                  <input v-model="bookingForm.appointmentDate" type="date" class="form-input" required />
                  <span v-if="bookingErrors.appointmentDate" class="form-error">{{ bookingErrors.appointmentDate }}</span>
                </div>
                <div class="form-group">
                  <label class="form-label">Khoa muốn khám (tùy chọn)</label>
                  <select v-model="selectedDepartmentId" class="form-select">
                    <option value="">Chọn khoa</option>
                    <option v-for="d in departments" :key="d.id" :value="d.id">{{ d.name }}</option>
                  </select>
                </div>
              </div>

              <div class="form-row">
                <div class="form-group">
                  <label class="form-label">Bác sĩ mong muốn (tùy chọn)</label>
                  <select v-model="bookingForm.doctorId" class="form-select">
                    <option value="">Không chọn bác sĩ</option>
                    <option v-for="doctor in filteredDoctors" :key="doctor.id" :value="doctor.id">
                      {{ doctor.fullName }}{{ doctor.code ? ` (${doctor.code})` : '' }}
                    </option>
                  </select>
                </div>
                <div class="form-group">
                  <label class="form-label">Khung giờ *</label>
                  <select v-model="bookingForm.appointmentTime" class="form-select" :disabled="slotLoading" required>
                    <option value="" disabled>
                      {{
                        slotLoading
                          ? 'Đang tải khung giờ...'
                          : availableSlots.length
                            ? 'Chọn khung giờ'
                            : 'Không có khung giờ, vui lòng đổi ngày hoặc bác sĩ'
                      }}
                    </option>
                    <option
                      v-for="slot in availableSlots"
                      :key="slot.id || slot.startTime"
                      :value="(slot.startTime || '').slice(0, 5)"
                      :disabled="slot.isBooked"
                    >
                      {{ slot.slotLabel || (slot.startTime || '').slice(0, 5) }}{{ slot.isBooked ? ' (Đã đặt)' : '' }}
                    </option>
                  </select>
                  <span v-if="bookingErrors.appointmentTime" class="form-error">{{ bookingErrors.appointmentTime }}</span>
                </div>
              </div>

              <div class="form-group">
                <label class="form-label">Lý do khám *</label>
                <textarea v-model="bookingForm.reason" class="form-textarea" required rows="4" placeholder="Vui lòng mô tả lý do khám của bạn"></textarea>
                <span v-if="bookingErrors.reason" class="form-error">{{ bookingErrors.reason }}</span>
              </div>

              <button type="submit" class="btn btn-primary w-100" :disabled="bookingLoading">
                <i v-if="!bookingLoading" class="bi bi-calendar-check me-2"></i>
                <i v-else class="bi bi-spinner animate-spin me-2"></i>
                {{ bookingLoading ? 'Đang đặt...' : 'Đặt lịch khám' }}
              </button>
            </form>

            <button v-if="bookingSuccess" @click="resetBookingForm" class="btn btn-secondary w-100 mt-2">
              <i class="bi bi-arrow-counterclockwise me-2"></i>Đặt lịch khám khác
            </button>
          </div>

          <div class="col-lg-5">
            <div class="intro-panel">
              <div class="intro-panel-icon" aria-hidden="true">
                <i class="bi bi-info-circle"></i>
              </div>
              <h4 class="intro-panel-title">Hướng dẫn nhanh</h4>
              <p class="text-muted mb-3">Điền form bên cạnh theo từng bước; sau khi thành công bạn sẽ nhận mã khám qua email.</p>
              <ul class="list-unstyled small">
                <li><i class="bi bi-check-circle text-success me-2"></i>Khung giờ hành chính 07:00–22:00</li>
                <li><i class="bi bi-check-circle text-success me-2"></i>Chọn khoa mong muốn (tùy chọn)</li>
                <li><i class="bi bi-check-circle text-success me-2"></i>OTP email để bảo vệ thông tin</li>
                <li><i class="bi bi-check-circle text-success me-2"></i>Nhận mã AppointmentCode để tra cứu/thu ngân</li>
              </ul>
              <div class="intro-panel-support">
                <div class="fw-semibold mb-1">Hỗ trợ</div>
                <div class="small text-muted">Hotline: 1900 1234 · Email: support@clinic.local</div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </section>

    <!-- Doctor showcase -->
    <DoctorShowcase />

    <!-- Facilities / News -->
    <FacilityHighlights />
    <NewsSection />

    <!-- Search -->
    <section id="search" class="search-section">
      <div class="container">
        <p class="section-eyebrow">Tra cứu</p>
        <h2 class="section-title">Tìm lịch khám & lịch đã tạo</h2>
        <p class="section-subtitle">Bên trái: tra cứu bằng mã + SĐT hoặc email. Bên phải: danh sách kết quả / lịch đã lưu.</p>

        <div class="row g-4 align-items-start">
          <div class="col-lg-6">
            <div v-if="searchError" class="alert alert-danger">
              <i class="bi bi-exclamation-circle me-2"></i>{{ searchError }}
            </div>

        <div v-if="searchResult" class="search-result">
          <div class="result-card">
            <div class="result-header">
              <h3>Chi tiết lịch khám</h3>
              <span class="badge" :class="`badge-${getStatusClass(searchResult.status)}`">{{ searchResult.status }}</span>
            </div>
                <div class="result-grid">
                  <div class="result-item"><label>Mã lịch khám</label><p class="result-value">{{ searchResult.appointmentCode }}</p></div>
                  <div class="result-item"><label>Tên bệnh nhân</label><p class="result-value">{{ searchResult.fullName }}</p></div>
                  <div class="result-item"><label>Ngày & Giờ</label><p class="result-value">{{ formatDateTime(searchResult.appointmentDate, searchResult.appointmentTime) }}</p></div>
                  <div class="result-item"><label>Điện thoại</label><p class="result-value">{{ searchResult.phone }}</p></div>
                  <div class="result-item"><label>Email</label><p class="result-value">{{ searchResult.email }}</p></div>
                  <div class="result-item"><label>Lý do</label><p class="result-value">{{ searchResult.reason }}</p></div>
                </div>
                <div class="mt-3 border rounded-3 p-3 bg-light">
                  <div class="d-flex justify-content-between align-items-start gap-2 mb-2">
                    <div>
                      <h4 class="h6 mb-1">Theo dõi hàng chờ</h4>
                      <p class="text-muted small mb-0">Số thứ tự sẽ xuất hiện sau khi nhân viên check-in tại quầy.</p>
                    </div>
                    <button class="btn btn-outline-secondary btn-sm" @click="loadPatientQueueStatus(searchResult.appointmentCode)">
                      <i class="bi bi-arrow-clockwise"></i>
                    </button>
                  </div>

                  <div v-if="queueStatusLoading" class="text-muted small">
                    <span class="spinner-border spinner-border-sm me-2"></span>Đang tải trạng thái hàng chờ...
                  </div>

                  <div v-else-if="queueStatusError" class="alert alert-warning py-2 mb-0">
                    {{ queueStatusError }}
                  </div>

                  <template v-else-if="patientQueueStatus">
                    <div v-if="patientQueueStatus.ownQueue" class="row g-3">
                      <div class="col-md-6">
                        <div class="border rounded-3 p-3 h-100">
                          <div class="text-muted small text-uppercase">Số thứ tự của bạn</div>
                          <div class="display-6 fw-bold mb-1">#{{ patientQueueStatus.ownQueue.queueNumber }}</div>
                          <div class="small">Phòng: {{ patientQueueStatus.ownQueue.roomName }}</div>
                          <div class="small text-muted">Trạng thái: {{ patientQueueStatus.ownQueue.status }}</div>
                        </div>
                      </div>
                      <div class="col-md-6">
                        <div class="border rounded-3 p-3 h-100">
                          <div class="text-muted small text-uppercase">Đang được gọi</div>
                          <div class="display-6 fw-bold mb-1">
                            {{ patientQueueStatus.currentCalling ? `#${patientQueueStatus.currentCalling.queueNumber}` : '—' }}
                          </div>
                          <div class="small" v-if="patientQueueStatus.currentCalling">
                            {{ patientQueueStatus.currentCalling.fullName }}
                          </div>
                          <div class="small text-muted">
                            {{ patientQueueStatus.currentCalling?.roomName || patientQueueStatus.ownQueue.roomName }}
                          </div>
                        </div>
                      </div>
                      <div class="col-12">
                        <div class="alert alert-info py-2 mb-0">
                          Còn <strong>{{ patientQueueStatus.waitingAhead }}</strong> người ở trước bạn.
                          <span v-if="patientQueueStatus.message"> {{ patientQueueStatus.message }}</span>
                        </div>
                      </div>
                    </div>

                    <div v-else class="alert alert-secondary py-2 mb-0">
                      {{ patientQueueStatus.message || 'Lịch khám này chưa được check-in vào hàng chờ.' }}
                    </div>
                  </template>
                </div>
                <div class="mt-3 border rounded-3 p-3 bg-light">
                  <div class="d-flex justify-content-between align-items-start gap-2 mb-2">
                    <div>
                      <h4 class="h6 mb-1">Theo dõi hàng chờ</h4>
                      <p class="text-muted small mb-0">Số thứ tự sẽ xuất hiện sau khi nhân viên check-in tại quầy.</p>
                    </div>
                    <button class="btn btn-outline-secondary btn-sm" @click="loadPatientQueueStatus(searchResult.appointmentCode)">
                      <i class="bi bi-arrow-clockwise"></i>
                    </button>
                  </div>

                  <div v-if="queueStatusLoading" class="text-muted small">
                    <span class="spinner-border spinner-border-sm me-2"></span>Đang tải trạng thái hàng chờ...
                  </div>

                  <div v-else-if="queueStatusError" class="alert alert-warning py-2 mb-0">
                    {{ queueStatusError }}
                  </div>

                  <template v-else-if="patientQueueStatus">
                    <div v-if="patientQueueStatus.ownQueue" class="row g-3">
                      <div class="col-md-6">
                        <div class="border rounded-3 p-3 h-100">
                          <div class="text-muted small text-uppercase">Số thứ tự của bạn</div>
                          <div class="display-6 fw-bold mb-1">#{{ patientQueueStatus.ownQueue.queueNumber }}</div>
                          <div class="small">Phòng: {{ patientQueueStatus.ownQueue.roomName }}</div>
                          <div class="small text-muted">Trạng thái: {{ patientQueueStatus.ownQueue.status }}</div>
                        </div>
                      </div>
                      <div class="col-md-6">
                        <div class="border rounded-3 p-3 h-100">
                          <div class="text-muted small text-uppercase">Đang được gọi</div>
                          <div class="display-6 fw-bold mb-1">
                            {{ patientQueueStatus.currentCalling ? `#${patientQueueStatus.currentCalling.queueNumber}` : '—' }}
                          </div>
                          <div class="small" v-if="patientQueueStatus.currentCalling">
                            {{ patientQueueStatus.currentCalling.fullName }}
                          </div>
                          <div class="small text-muted">
                            {{ patientQueueStatus.currentCalling?.roomName || patientQueueStatus.ownQueue.roomName }}
                          </div>
                        </div>
                      </div>
                      <div class="col-12">
                        <div class="alert alert-info py-2 mb-0">
                          Còn <strong>{{ patientQueueStatus.waitingAhead }}</strong> người ở trước bạn.
                          <span v-if="patientQueueStatus.message"> {{ patientQueueStatus.message }}</span>
                        </div>
                      </div>
                    </div>

                    <div v-else class="alert alert-secondary py-2 mb-0">
                      {{ patientQueueStatus.message || 'Lịch khám này chưa được check-in vào hàng chờ.' }}
                    </div>
                  </template>
                </div>
            <div class="result-actions">
              <button @click="cancelAppointment" class="btn btn-danger" :disabled="cancelLoading">
                <span v-if="cancelLoading" class="spinner-border spinner-border-sm me-1"></span>
                <i v-else class="bi bi-x-circle me-2"></i>Huỷ lịch khám
              </button>
              <button @click="resetSearch" class="btn btn-secondary"><i class="bi bi-search me-2"></i>Tìm kiếm khác</button>
            </div>
          </div>
        </div>
        <div v-else-if="searchResults.length" class="search-result">
          <div class="result-card">
            <div class="result-header">
              <h3>Chọn một lịch để xem chi tiết</h3>
            </div>
            <p class="text-muted small mb-0">Nhấp vào một dòng ở bảng bên phải để xem chi tiết.</p>
          </div>
        </div>

            <form v-else @submit.prevent="submitSearch" class="search-form">
              <div class="form-row">
                <div class="form-group">
                  <label class="form-label">Mã lịch khám</label>
                  <input v-model="searchForm.appointmentCode" type="text" class="form-input" placeholder="Tùy chọn: để trống để xem tất cả lịch" />
                </div>
                <div class="form-group">
                  <label class="form-label">Số điện thoại</label>
                  <input
                    v-model="searchForm.phone"
                    type="tel"
                    class="form-input"
                    placeholder="Nhập SĐT hoặc email"
                    inputmode="numeric"
                    maxlength="11"
                    @input="searchForm.phone = normalizePhoneInput(searchForm.phone)"
                  />
                </div>
                <div class="form-group">
                  <label class="form-label">Email</label>
                  <input v-model="searchForm.email" type="email" class="form-input" placeholder="Nếu không có SĐT" />
                </div>
              </div>
              <button type="submit" class="btn btn-primary" :disabled="searchLoading">
                <i v-if="!searchLoading" class="bi bi-search me-2"></i>
                <i v-else class="bi bi-spinner animate-spin me-2"></i>
                {{ searchLoading ? 'Đang tìm kiếm...' : 'Tìm lịch khám' }}
              </button>
            </form>
          </div>

          <div class="col-lg-6">
            <div class="search-result">
              <div class="result-card">
                <div class="result-header">
                  <h3>{{ searchResults.length ? 'Kết quả tìm kiếm' : 'Danh sách đã lưu' }}</h3>
                  <span class="badge badge-info">{{ searchResults.length || recentAppointments.length }}</span>
                </div>
                <div class="table-responsive">
                  <table class="table table-sm align-middle mb-0">
                    <thead>
                      <tr>
                        <th>Mã</th>
                        <th>Ngày giờ</th>
                        <th>Trạng thái</th>
                      </tr>
                    </thead>
                    <tbody>
                      <tr
                        v-for="item in (searchResults.length ? searchResults : recentAppointments)"
                        :key="item.appointmentCode"
                        :class="{ 'table-active': searchResult?.appointmentCode === item.appointmentCode }"
                        style="cursor:pointer"
                        @click="selectSearchResult(item)"
                      >
                        <td class="text-monospace">{{ item.appointmentCode }}</td>
                        <td class="small">{{ formatDateTime(item.appointmentDate, item.appointmentTime) }}</td>
                        <td><span class="badge" :class="`badge-${getStatusClass(item.status)}`">{{ item.status }}</span></td>
                      </tr>
                      <tr v-if="!searchResults.length && recentAppointments.length === 0">
                        <td colspan="3" class="text-muted small text-center">Chưa có lịch nào được lưu.</td>
                      </tr>
                      <tr v-if="searchResults.length === 0 && (searchForm.phone || searchForm.email || searchForm.appointmentCode)">
                        <td colspan="3" class="text-muted small text-center">Không tìm thấy lịch nào.</td>
                      </tr>
                    </tbody>
                  </table>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </section>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted, onBeforeUnmount, nextTick, computed, watch } from 'vue'
import api from '@/services/api'
import { doctorScheduleService } from '@/services/doctorScheduleService'
import { queueService, type PatientQueueStatus } from '@/services/queueService'
import { buildBusinessHourSlots } from '@/utils/appointmentSlots'
import { toLocalDateInputValue } from '@/utils/date'
import '@/styles/layouts/guest-dashboard.css'
import DoctorShowcase from '@/components/landing/DoctorShowcase.vue'
import FacilityHighlights from '@/components/landing/FacilityHighlights.vue'
import NewsSection from '@/components/landing/NewsSection.vue'

/* Services display */
const services = reactive([
  { id: 1, name: 'Nội tổng quát', description: 'Khám tổng quát, tư vấn sức khỏe cá nhân hóa', icon: 'bi bi-activity', color: 'blue' },
  { id: 2, name: 'Nhi khoa', description: 'Khám trẻ em, theo dõi vaccine, tư vấn dinh dưỡng', icon: 'bi bi-emoji-smile', color: 'sky' },
  { id: 3, name: 'Tim mạch', description: 'Khám tim mạch, tầm soát nguy cơ, đo điện tim', icon: 'bi bi-heart-pulse', color: 'red' },
  { id: 4, name: 'Chẩn đoán hình ảnh', description: 'Siêu âm, X-quang, hỗ trợ bác sĩ chẩn đoán', icon: 'bi bi-camera-reels', color: 'blue' },
  { id: 5, name: 'Xét nghiệm', description: 'Lấy mẫu và trả kết quả nhanh, tích hợp hồ sơ khám', icon: 'bi bi-droplet', color: 'sky' },
  { id: 6, name: 'Tiêm chủng', description: 'Tư vấn và tiêm vaccine theo lịch cá nhân', icon: 'bi bi-shield-plus', color: 'red' }
])

/* Booking state */
const bookingForm = reactive({
  fullName: '',
  dateOfBirth: '',
  gender: '',
  phone: '',
  email: '',
  address: '',
  CitizenId: '', // Thêm mới
  insuranceNumber: '', // Thêm mới
  appointmentDate: toLocalDateInputValue(),
  doctorId: '',
  appointmentTime: '',
  reason: ''
})

const bookingErrors = reactive<Record<string, string>>({
  fullName: '',
  dateOfBirth: '',
  gender: '',
  phone: '',
  email: '',
  address: '',
  appointmentDate: '',
  doctorId: '',
  appointmentTime: '',
  reason: ''
})

const bookingLoading = ref(false)
const bookingError = ref('')
const bookingSuccess = ref(false)
const bookingResponse = ref({ appointmentCode: '', status: '', appointmentDate: '', appointmentTime: '' })
const bookingSuccessRef = ref<HTMLElement | null>(null)

/* OTP */
const otpCode = ref('')
const otpSent = ref(false)
const otpVerified = ref(false)
const otpSending = ref(false)
const otpVerifying = ref(false)
const otpError = ref('')
const countdown = ref(0)
let countdownTimer: any = null

/* Returning patient lookup */
const lookupPhone = ref('')
const lookupEmail = ref('')
const lookupLoading = ref(false)
const lookupError = ref('')
const isReturning = ref(false)
const showReturningLookup = ref(false)

/* Search */
const searchForm = reactive({ appointmentCode: '', phone: '', email: '' })
const searchLoading = ref(false)
const searchError = ref('')
const searchResult = ref<any>(null)
const searchResults = ref<any[]>([])
const cancelLoading = ref(false)
const cancelError = ref('')
const recentAppointments = ref<any[]>([])
const patientQueueStatus = ref<PatientQueueStatus | null>(null)
const queueStatusLoading = ref(false)
const queueStatusError = ref('')
let queueStatusTimer: ReturnType<typeof setInterval> | null = null

/* Departments (optional selection) */
const departments = ref<any[]>([])
const selectedDepartmentId = ref('')
const doctors = ref<any[]>([])
const availableSlots = ref<any[]>([])
const slotLoading = ref(false)

const isDoctorActive = (status: unknown) => {
  if (status === null || status === undefined) return true
  if (typeof status === 'number') return status !== 2
  const normalized = String(status).toLowerCase()
  return normalized !== 'inactive'
}

const filteredDoctors = computed(() =>
  doctors.value.filter((doctor) =>
    isDoctorActive(doctor.status) &&
    (!selectedDepartmentId.value || doctor.departmentId === selectedDepartmentId.value)
  )
)

const normalizePhoneInput = (value: string) => value.replace(/\D/g, '').slice(0, 11)

/* Helpers */
const scrollToBooking = () => document.getElementById('booking')?.scrollIntoView({ behavior: 'smooth' })
const scrollToSearch = () => document.getElementById('search')?.scrollIntoView({ behavior: 'smooth' })

const formatDateTime = (dateStr: string, timeStr: string) => {
  if (!dateStr || !timeStr) return ''
  const date = new Date(dateStr)
  const [h, m] = timeStr.split(':')
  return `${date.toLocaleDateString()} ${h}:${m}`
}

/* Recent appointments (local storage) */
const RECENT_KEY = 'recentAppointments'
const loadRecent = () => {
  try {
    const raw = localStorage.getItem(RECENT_KEY)
    recentAppointments.value = raw ? JSON.parse(raw) : []
  } catch {
    recentAppointments.value = []
  }
}
const saveRecent = () => localStorage.setItem(RECENT_KEY, JSON.stringify(recentAppointments.value.slice(0, 20)))
const addRecent = (item: any) => {
  if (!item?.appointmentCode) return
  const normalized = {
    appointmentCode: item.appointmentCode,
    appointmentDate: item.appointmentDate,
    appointmentTime: item.appointmentTime,
    status: item.status || 'Pending'
  }
  recentAppointments.value = [
    normalized,
    ...recentAppointments.value.filter((x) => x.appointmentCode !== normalized.appointmentCode)
  ].slice(0, 20)
  saveRecent()
}

const validateBookingForm = () => {
  Object.keys(bookingErrors).forEach((k) => (bookingErrors[k] = ''))
  const todayStr = toLocalDateInputValue()

  const setError = (field: keyof typeof bookingErrors, message: string) => {
    bookingErrors[field] = message
  }

  if (!bookingForm.fullName.trim()) setError('fullName', 'Họ và tên là bắt buộc')
  if (!bookingForm.dateOfBirth) setError('dateOfBirth', 'Ngày sinh là bắt buộc')
  if (!bookingForm.gender) setError('gender', 'Giới tính là bắt buộc')

  if (!bookingForm.phone.trim()) setError('phone', 'Số điện thoại là bắt buộc')
  else if (!/^[0-9]{9,11}$/.test(bookingForm.phone)) setError('phone', 'Số điện thoại phải có 9-11 chữ số')

  if (!bookingForm.email.trim()) setError('email', 'Email là bắt buộc')
  else if (!/\S+@\S+\.\S+/.test(bookingForm.email)) setError('email', 'Định dạng email không hợp lệ')

  if (!bookingForm.address.trim()) setError('address', 'Địa chỉ là bắt buộc')

  if (!bookingForm.appointmentDate) setError('appointmentDate', 'Ngày khám là bắt buộc')
  else if (bookingForm.appointmentDate < todayStr) setError('appointmentDate', 'Chỉ được đặt từ hôm nay trở đi')

  if (!bookingForm.appointmentTime) setError('appointmentTime', 'Thời gian khám là bắt buộc')
  if (!bookingForm.reason.trim()) setError('reason', 'Lý do khám là bắt buộc')

  return !Object.values(bookingErrors).some(Boolean)
}

const mapGenderToOption = (value: any) => {
  if (value === null || value === undefined) return ''
  if (typeof value === 'number') return String(value)
  const normalized = value.toString().toLowerCase()
  if (normalized.startsWith('m')) return '1'
  if (normalized.startsWith('f')) return '2'
  return ''
}

const applyPrefill = (data: any) => {
  bookingForm.fullName = data.fullName || ''
  bookingForm.dateOfBirth = data.dateOfBirth?.slice(0, 10) || ''
  bookingForm.gender = mapGenderToOption(data.gender)
  bookingForm.phone = normalizePhoneInput(data.phone || '')
  bookingForm.email = data.email || ''
  bookingForm.address = data.address || ''
  bookingForm.CitizenId = data.citizenId || '' // Điền CCCD cũ
  bookingForm.insuranceNumber = data.insuranceCardNumber || '' // Điền BHYT cũ
  isReturning.value = true
}

const clearPrefill = () => {
  isReturning.value = false
  lookupError.value = ''
}

const loadDoctors = async () => {
  try {
    const res = await api.get('/Doctor')
    doctors.value = Array.isArray(res.data) ? res.data : []
  } catch (err) {
    console.warn('Không tải được danh sách bác sĩ', err)
    doctors.value = []
  }
}

const loadAvailableSlots = async () => {
  availableSlots.value = []

  if (!bookingForm.appointmentDate) {
    bookingForm.appointmentTime = ''
    return
  }

  if (!bookingForm.doctorId) {
    const slots = buildBusinessHourSlots(bookingForm.appointmentDate)
    availableSlots.value = slots

    const selectedSlot = slots.find((slot) => slot.startTime?.startsWith(bookingForm.appointmentTime))
    if (!selectedSlot) {
      bookingForm.appointmentTime = ''
    }
    return
  }

  try {
    slotLoading.value = true
     const slots = await doctorScheduleService.getAvailableSlots(
       bookingForm.doctorId,
       bookingForm.appointmentDate
     )
     availableSlots.value = slots

    const selectedSlot = slots.find((slot) => slot.startTime?.startsWith(bookingForm.appointmentTime))
    if (!selectedSlot || selectedSlot.isBooked) {
      bookingForm.appointmentTime = ''
    }
  } catch (err) {
    console.warn('Không tải được slot trống', err)
    bookingForm.appointmentTime = ''
    availableSlots.value = []
  } finally {
    slotLoading.value = false
  }
}

const lookupPatient = async () => {
  lookupError.value = ''
  if (!lookupPhone.value.trim() && !lookupEmail.value.trim()) {
    lookupError.value = 'Nhập SĐT hoặc email để tra cứu'
    return
  }
  try {
    lookupLoading.value = true
    const res = await api.get('/appointments/patient-lookup', {
      params: { phone: lookupPhone.value || undefined, email: lookupEmail.value || undefined }
    })
    applyPrefill(res.data)
  } catch (err: any) {
    console.error(err)
    lookupError.value = err?.response?.data?.message || 'Không tìm thấy bệnh nhân phù hợp'
  } finally {
    lookupLoading.value = false
  }
}

/* OTP */
const startCountdown = () => {
  if (countdownTimer) clearInterval(countdownTimer)
  countdown.value = 60
  countdownTimer = setInterval(() => {
    countdown.value -= 1
    if (countdown.value <= 0 && countdownTimer) {
      clearInterval(countdownTimer)
      countdownTimer = null
    }
  }, 1000)
}

const sendOtp = async () => {
  otpError.value = ''
  if (!bookingForm.email?.trim()) {
    otpError.value = 'Vui lòng nhập email trước khi gửi OTP'
    return
  }
  try {
    otpSending.value = true
    await api.post('/email/send-otp', { email: bookingForm.email })
    otpSent.value = true
    otpVerified.value = false
    startCountdown()
  } catch (err: any) {
    console.error(err)
    otpError.value = err?.response?.data?.message || 'Không gửi được OTP, thử lại sau'
  } finally {
    otpSending.value = false
  }
}

const verifyOtp = async () => {
  otpError.value = ''
  if (!otpCode.value.trim()) {
    otpError.value = 'Nhập mã OTP'
    return
  }
  try {
    otpVerifying.value = true
    await api.post('/email/verify-otp', { email: bookingForm.email, code: otpCode.value })
    otpVerified.value = true
    otpError.value = ''
  } catch (err: any) {
    console.error(err)
    otpError.value = err?.response?.data?.message || 'OTP sai hoặc đã hết hạn'
    otpVerified.value = false
  } finally {
    otpVerifying.value = false
  }
}

/* Booking submit */
const submitBooking = async () => {
  if (!validateBookingForm()) return
  if (!otpVerified.value) { bookingError.value = 'Vui lòng gửi và xác thực OTP email trước khi đặt lịch'; return }

  try {
    bookingLoading.value = true
    bookingError.value = ''
    const deptName = departments.value.find((d: any) => d.id === selectedDepartmentId.value)?.name
    const reasonWithDept = deptName
      ? `Khoa yêu cầu: ${deptName}${bookingForm.reason ? ' | ' + bookingForm.reason : ''}`
      : bookingForm.reason
    const response = await api.post('/appointments', {
      fullName: bookingForm.fullName,
      dateOfBirth: bookingForm.dateOfBirth,
      gender: parseInt(bookingForm.gender),
      phone: bookingForm.phone,
      email: bookingForm.email,
      address: bookingForm.address,
      citizenId: bookingForm.CitizenId,
      insuranceCardNumber: bookingForm.insuranceNumber,
      appointmentDate: bookingForm.appointmentDate,
      doctorId: bookingForm.doctorId || null,
      appointmentTime: bookingForm.appointmentTime + ':00',
      reason: reasonWithDept
    })
    bookingResponse.value = response.data
    bookingSuccess.value = true
    await nextTick()
    bookingSuccessRef.value?.scrollIntoView({ behavior: 'smooth', block: 'start' })
    addRecent(response.data)
  } catch (error: any) {
    const raw = error?.response?.data
    const msg =
      typeof raw === 'string'
        ? raw
        : (raw?.message || raw?.error || raw?.title || null)
    bookingError.value = msg || 'Không thể đặt lịch khám. Vui lòng thử lại.'
    console.error('Booking error:', error)
  } finally {
    bookingLoading.value = false
  }
}

const resetBookingForm = () => {
  Object.keys(bookingForm).forEach((k) => (bookingForm as any)[k] = '')
  bookingForm.appointmentDate = toLocalDateInputValue()
  bookingSuccess.value = false
  bookingError.value = ''
  otpCode.value = ''
  otpSent.value = false
  otpVerified.value = false
  otpError.value = ''
  countdown.value = 0
  selectedDepartmentId.value = ''
  availableSlots.value = []
  loadAvailableSlots()
}

/* Search */
const submitSearch = async () => {
  if (!searchForm.phone.trim() && !searchForm.email.trim()) {
    searchError.value = 'Nhập SĐT hoặc email (mã lịch tùy chọn)'
    return
  }
  try {
    searchLoading.value = true
    searchError.value = ''
    const response = await api.post('/appointments/search', {
      appointmentCode: searchForm.appointmentCode || undefined,
      phone: searchForm.phone || undefined,
      email: searchForm.email || undefined
    })
    const list = Array.isArray(response.data) ? response.data : []
    searchResults.value = list
    searchResult.value = list.length === 1 ? list[0] : null
    if (!list.length) {
      searchError.value = 'Không tìm thấy lịch khám'
    } else if (list.length === 1) {
      addRecent(list[0])
    }
  } catch (error: any) {
    searchError.value = error.response?.data?.message || 'Không tìm thấy lịch khám'
  } finally {
    searchLoading.value = false
  }
}

const cancelAppointment = async () => {
  if (!searchResult.value) return
  try {
    cancelLoading.value = true
    await api.post('/appointments/cancel', {
      appointmentCode: searchResult.value.appointmentCode,
      fullName: searchResult.value.fullName,
      phone: searchResult.value.phone
    })
    addRecent({ ...searchResult.value, status: 'Cancelled' })
    resetSearch()
  } catch (error: any) {
    cancelError.value = error.response?.data?.message || 'Không thể huỷ lịch'
  } finally {
    cancelLoading.value = false
  }
}

const resetSearch = () => {
  searchResult.value = null
  searchResults.value = []
  searchError.value = ''
  patientQueueStatus.value = null
  queueStatusError.value = ''
  clearQueueStatusPolling()
  searchForm.appointmentCode = ''
  searchForm.phone = ''
  searchForm.email = ''
}

const selectSearchResult = (item: any) => {
  searchResult.value = item
  addRecent(item)
}

const clearQueueStatusPolling = () => {
  if (queueStatusTimer) {
    clearInterval(queueStatusTimer)
    queueStatusTimer = null
  }
}

const loadPatientQueueStatus = async (appointmentCode?: string) => {
  const code = appointmentCode || searchResult.value?.appointmentCode
  if (!code) {
    patientQueueStatus.value = null
    queueStatusError.value = ''
    clearQueueStatusPolling()
    return
  }

  try {
    queueStatusLoading.value = true
    queueStatusError.value = ''
    patientQueueStatus.value = await queueService.getPatientStatus(code)
  } catch (error: any) {
    patientQueueStatus.value = null
    queueStatusError.value = error?.response?.data?.message || 'Không tải được trạng thái hàng chờ'
  } finally {
    queueStatusLoading.value = false
  }
}

const getStatusClass = (status: string) => {
  switch (status?.toLowerCase()) {
    case 'pending': return 'warning'
    case 'confirmed': return 'info'
    case 'completed': return 'success'
    case 'cancelled': return 'danger'
    default: return 'secondary'
  }
}

const departmentNameMap: Record<string, string> = {
  'General Medicine': 'Nội tổng quát',
  Surgery: 'Ngoại khoa',
  Pediatrics: 'Nhi khoa',
  ENT: 'Tai Mũi Họng',
  Obstetrics: 'Sản phụ khoa',
  Diagnostics: 'Cận lâm sàng'
}

const toVietnameseDepartmentName = (name?: string) => {
  if (!name) return ''
  return departmentNameMap[name.trim()] || name
}

/* Load departments for optional selection */
const loadDepartments = async () => {
  try {
    const res = await api.get('/Departments')
    const rawDepartments = Array.isArray(res.data) ? res.data : []
    departments.value = rawDepartments.map((department: any) => ({
      ...department,
      name: toVietnameseDepartmentName(department?.name)
    }))
  } catch (err) {
    console.warn('Không tải được danh sách khoa', err)
  }
}

watch(selectedDepartmentId, () => {
  if (bookingForm.doctorId && !filteredDoctors.value.some((doctor) => doctor.id === bookingForm.doctorId)) {
    bookingForm.doctorId = ''
    bookingForm.appointmentTime = ''
    availableSlots.value = []
  }
})

watch([() => bookingForm.doctorId, () => bookingForm.appointmentDate], () => {
  loadAvailableSlots()
})

watch(
  () => searchResult.value?.appointmentCode,
  async (appointmentCode) => {
    clearQueueStatusPolling()

    if (!appointmentCode) {
      patientQueueStatus.value = null
      queueStatusError.value = ''
      return
    }

    await loadPatientQueueStatus(appointmentCode)
    queueStatusTimer = setInterval(() => {
      loadPatientQueueStatus(appointmentCode)
    }, 10000)
  }
)

onMounted(() => {
  loadDepartments()
  loadDoctors()
  loadAvailableSlots()
  loadRecent()
})

onBeforeUnmount(() => {
  clearQueueStatusPolling()
})
</script>
