-- Additional realistic test data for AI-Powered Software Development
-- Run this using SQLite browser or add via application UI

-- Additional Projects
INSERT INTO Projects (Name, Description, RepositoryUrl, Technology, CreatedAt, UpdatedAt)
VALUES 
('E-Commerce Platform', 'Online shopping platform with AI-powered recommendation engine', 'https://github.com/example/ecommerce', 'GitHub Copilot', '2026-04-15', '2026-05-01'),
('Healthcare Management System', 'HIPAA-compliant healthcare system with AI diagnostics', 'https://github.com/example/healthcare', 'Claude Code', '2026-03-20', '2026-04-28'),
('FinTech Mobile App', 'Mobile banking app with fraud detection AI', 'https://github.com/example/fintech', 'Cursor AI', '2026-02-10', '2026-05-05');

-- Additional Code Reviews with realistic scenarios
INSERT INTO CodeReviews (ProjectId, Title, CodeSnippet, AiReviewResult, ReviewType, Severity, Status, ReviewedAt)
VALUES 
(4, 'SQL Injection Prevention', 'string query = "SELECT * FROM Users WHERE Username = '" + username + "'";', 
'CRITICAL: SQL injection vulnerability. Use parameterized queries or ORM instead of string concatenation for SQL commands.', 
'Security', 'Critical', 'Completed', '2026-05-02'),

(4, 'Async/Await Pattern', 'public void ProcessOrder() { var result = GetData().Result; }',
'WARNING: Blocking async code with .Result can cause deadlocks. Use await keyword instead.',
'Best Practices', 'High', 'Completed', '2026-05-03'),

(5, 'Patient Data Encryption', 'string patientData = JsonConvert.SerializeObject(patient); File.WriteAllText("patient.json", patientData);',
'CRITICAL: Patient data stored in plain text violates HIPAA. Implement encryption for PHI before storage.',
'Security', 'Critical', 'In Progress', '2026-04-25'),

(6, 'Transaction Validation', 'if (amount > 0) { ProcessTransaction(amount); }',
'MEDIUM: Missing fraud detection checks. Integrate AI fraud detection before processing high-value transactions.',
'Security', 'Medium', 'Pending', '2026-05-04');

-- Additional Bug Reports
INSERT INTO BugReports (ProjectId, Title, Description, AiDetectionResult, Severity, Status, DetectedAt, ResolvedAt)
VALUES 
(4, 'Cart Total Calculation Error', 'Shopping cart showing incorrect total when applying multiple discount codes',
'AI Detection: Floating-point precision error in price calculation. Use decimal type instead of double for currency values.',
'High', 'Resolved', '2026-04-20', '2026-04-25'),

(5, 'Data Race in Patient Records', 'Concurrent access to patient records causing data inconsistency',
'AI Detection: Race condition detected. Implement locking mechanism or use thread-safe collections for patient data access.',
'Critical', 'In Progress', '2026-04-22', NULL),

(6, 'Session Timeout Not Working', 'User sessions not expiring after period of inactivity',
'AI Detection: Session timeout configuration missing or incorrect. Check middleware configuration and session state management.',
'Medium', 'Open', '2026-05-01', NULL);

-- Additional Test Cases
INSERT INTO TestCases (ProjectId, TestName, TestDescription, AiGeneratedCode, TestType, Status, GeneratedAt, LastRunAt)
VALUES 
(4, 'AddToCart_ValidProduct_ReturnsSuccess', 'Test adding valid product to shopping cart',
'[Test]\npublic void AddToCart_ValidProduct_ReturnsSuccess() {\n    var cart = new ShoppingCart();\n    var product = new Product { Id = 1, Price = 99.99m };\n    var result = cart.AddItem(product, 1);\n    Assert.IsTrue(result.Success);\n    Assert.AreEqual(1, cart.Items.Count);\n}',
'Unit Test', 'Passed', '2026-04-18', '2026-05-03'),

(5, 'EncryptPatientData_SensitiveInfo_EncryptsCorrectly', 'Test patient data encryption for HIPAA compliance',
'[Test]\npublic void EncryptPatientData_SensitiveInfo_EncryptsCorrectly() {\n    var patient = new Patient { SSN = "123-45-6789", Name = "John Doe" };\n    var encrypted = EncryptionService.Encrypt(patient);\n    Assert.IsNotNull(encrypted.SSN);\n    Assert.AreNotEqual("123-45-6789", encrypted.SSN);\n}',
'Security Test', 'Passed', '2026-04-21', '2026-04-28'),

(6, 'FraudDetection_HighAmount_TriggersAlert', 'Test AI fraud detection for high-value transactions',
'[Test]\npublic void FraudDetection_HighAmount_TriggersAlert() {\n    var transaction = new Transaction { Amount = 50000, Type = "Wire" };\n    var result = FraudDetector.Analyze(transaction);\n    Assert.IsTrue(result.IsFlagged);\n    Assert.Contains("High Amount", result.Flags);\n}',
'Integration Test', 'Failed', '2026-05-02', '2026-05-04');

-- Additional DevOps Tasks
INSERT INTO DevOpsTasks (ProjectId, TaskName, Description, AiSuggestion, TaskType, Status, CreatedAt, CompletedAt)
VALUES 
(4, 'Setup Payment Gateway Integration', 'Integrate Stripe payment processing with AI fraud checks',
'AI Suggestion: Use Stripe.NET library with webhook handlers. Implement AI-based transaction anomaly detection using Azure ML.',
'CI/CD Automation', 'In Progress', '2026-04-25', NULL),

(5, 'HIPAA Compliance Audit', 'Automated compliance checking for healthcare regulations',
'AI Suggestion: Implement automated scans using OWASP ZAP. Use Claude to analyze code changes for PHI exposure risks.',
'Code Review Automation', 'Pending', '2026-04-26', NULL),

(6, 'Real-time Fraud Monitoring', 'Setup real-time transaction monitoring dashboard',
'AI Suggestion: Use SignalR for real-time updates. Integrate OpenAI Codex to generate custom fraud detection rules based on patterns.',
'Monitoring', 'Pending', '2026-05-03', NULL);
