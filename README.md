# AI Resume Analyzer

## 📌 Project Overview
The **AI Resume Analyzer** is a web-based platform that helps:
- **HR Professionals** filter and rank resumes based on **ATS scores**.
- **Job Seekers** optimize their resumes for better **Applicant Tracking System (ATS) compatibility**.

---

## 🚀 Features & Workflow

### 🔹 HR Section (Bulk Resume Analysis)
**Users**: HR professionals/recruiters.  
**Functionality**:
1. Upload **multiple resumes** (PDF/DOCX).
2. Upload a **job description** (JD).
3. AI **parses resumes & JD** and assigns **ATS scores**.
4. Resumes are **sorted** by the highest **ATS match**.
5. Download sorted resumes or view improvement suggestions.

### 🔹 Student Section (Single Resume Optimization)
**Users**: Job seekers & students.  
**Functionality**:
1. Upload **a single resume** (PDF/DOCX).
2. Enter the **desired job title**.
3. AI **analyzes the resume** and provides:
   - ✅ **ATS score** (how well it matches the job).
   - ✅ **Formatting corrections** (structure, font, readability).
   - ✅ **Improvement tips** (keywords, missing sections).

### 🔹 Admin Panel
**Users**: Admins.  
**Functionality**:
- Manage **users & uploaded data**.
- Monitor **AI analysis & reports**.
- Update **rules & configurations** for resume scoring.

---

## 🛠️ Tech Stack
- **Frontend**: Angular (UI, routing, form handling).
- **Backend**: .NET (API, resume processing, AI logic).
- **Database**: MongoDB (resume storage, user data).
- **AI Integration**: Google Gemini AI (GPT for parsing & scoring resumes).
- **Authentication**: JWT (secure user login & access control).

---

## 🔧 Installation & Setup
### 📍 Prerequisites
- Node.js & npm installed.
- Angular CLI installed.
- .NET SDK installed.
- MongoDB setup.

### 📍 Backend Setup (.NET API)
```bash
cd backend
# Install dependencies
dotnet restore
# Run backend
dotnet run
```

### 📍 Frontend Setup (Angular UI)
```bash
cd frontend
# Install dependencies
npm install
# Run Angular app
ng serve
```

---

## 🔐 Authentication
- **JWT Authentication** is used for user access control.
- Users need to **register/login** to access features.

---

## 📝 Additional Features
- **Chat with PDF** (AI analyzes resumes & provides insights).
- **Responsive UI** (works on all devices).
- **Secure file uploads** (privacy-focused resume processing).
- **Scalable architecture** (supports multiple users).

---

## 📌 Future Enhancements
- 📊 **More detailed AI insights** into resume strengths & weaknesses.
- 📂 **Resume template suggestions** based on job roles.
- 📡 **API integration** with LinkedIn for profile optimization.

---

## 🤝 Contributing
Want to contribute? Feel free to fork and submit a pull request! 🚀

