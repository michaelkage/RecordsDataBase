import React, { useEffect, useMemo, useState } from 'react';
import {
  Alert,
  FlatList,
  KeyboardAvoidingView,
  Modal,
  Platform,
  SafeAreaView,
  ScrollView,
  StyleSheet,
  Text,
  TextInput,
  TouchableOpacity,
  View,
} from 'react-native';
import AsyncStorage from '@react-native-async-storage/async-storage';

const STORAGE_KEY = 'students_db';
const ADMIN_PASSWORD = 'admin123password'; // TODO: replace with secure credential storage before production.

const EMPTY_FORM = {
  surname: '', fullName: '', department: '', classLevel: '', classArm: '',
};

const DEFAULT_DATA = [{
  id: 'BHS-2026-001',
  surname: 'Ayeni',
  fullName: 'Ayodeji Ayeni',
  department: 'Science',
  classLevel: 'SS3',
  classArm: 'Crimson',
  subjects: ['Mathematics', 'English Language', 'Civic Education', 'Physics', 'Chemistry'],
  scores: { Mathematics: 80, 'English Language': 75, 'Civic Education': 70, Physics: 85, Chemistry: 65 },
}];

export default function App() {
  const [portalMode, setPortalMode] = useState('student');
  const [loginId, setLoginId] = useState('');
  const [password, setPassword] = useState('');
  const [currentUser, setCurrentUser] = useState(null);
  const [students, setStudents] = useState([]);
  const [storageError, setStorageError] = useState(null);
  const [formVisible, setFormVisible] = useState(false);
  const [editingStudent, setEditingStudent] = useState(null);
  const [form, setForm] = useState(EMPTY_FORM);

  useEffect(() => { loadData(); }, []);

  const notifyStorageError = (operation, error) => {
    const message = error instanceof Error ? error.message : String(error);
    console.error(`Failed to ${operation}.`, error);
    setStorageError(`Local database error while trying to ${operation}.`);
    Alert.alert('Local database error', `The app could not ${operation}. Your existing data was not intentionally deleted.\n\n${message}`);
  };

  const saveStudents = async (nextStudents) => {
    try {
      await AsyncStorage.setItem(STORAGE_KEY, JSON.stringify(nextStudents));
      setStudents(nextStudents);
      setStorageError(null);
      return true;
    } catch (error) {
      notifyStorageError('save student records', error);
      return false;
    }
  };

  const loadData = async () => {
    try {
      const stored = await AsyncStorage.getItem(STORAGE_KEY);
      if (stored) {
        try {
          const parsed = JSON.parse(stored);
          if (!Array.isArray(parsed)) throw new Error('Stored student data is not an array.');
          setStudents(parsed);
          setStorageError(null);
          return;
        } catch (parseError) {
          notifyStorageError('read the local database', parseError);
          return;
        }
      }

      await AsyncStorage.setItem(STORAGE_KEY, JSON.stringify(DEFAULT_DATA));
      setStudents(DEFAULT_DATA);
      setStorageError(null);
    } catch (error) {
      notifyStorageError('load the local database', error);
    }
  };

  const generateStudentId = () => {
    const year = new Date().getFullYear();
    const highest = students.reduce((max, student) => {
      const match = String(student.id).match(/-(\d+)$/);
      return match ? Math.max(max, Number(match[1])) : max;
    }, 0);
    return `BHS-${year}-${String(highest + 1).padStart(3, '0')}`;
  };

  const openAddStudent = () => {
    setEditingStudent(null);
    setForm(EMPTY_FORM);
    setFormVisible(true);
  };

  const openEditStudent = (student) => {
    setEditingStudent(student);
    setForm({
      surname: student.surname || '', fullName: student.fullName || '',
      department: student.department || '', classLevel: student.classLevel || '', classArm: student.classArm || '',
    });
    setFormVisible(true);
  };

  const validateForm = () => {
    const required = ['surname', 'fullName', 'department', 'classLevel', 'classArm'];
    const missing = required.filter(key => !String(form[key]).trim());
    if (missing.length) {
      Alert.alert('Missing information', 'Please complete all student fields.');
      return false;
    }
    return true;
  };

  const saveStudent = async () => {
    if (!validateForm()) return;
    const normalized = Object.fromEntries(Object.entries(form).map(([key, value]) => [key, value.trim()]));

    if (editingStudent) {
      const next = students.map(student => student.id === editingStudent.id ? { ...student, ...normalized } : student);
      if (await saveStudents(next)) {
        setFormVisible(false);
        Alert.alert('Student updated', `${normalized.fullName} has been updated.`);
      }
      return;
    }

    const newStudent = {
      id: generateStudentId(),
      ...normalized,
      subjects: [],
      scores: {},
    };
    if (await saveStudents([newStudent, ...students])) {
      setFormVisible(false);
      Alert.alert('Student registered', `${newStudent.fullName} was registered as ${newStudent.id}.`);
    }
  };

  const deleteStudent = (student) => {
    Alert.alert('Delete student?', `Remove ${student.fullName} (${student.id}) from this device?`, [
      { text: 'Cancel', style: 'cancel' },
      { text: 'Delete', style: 'destructive', onPress: async () => {
        const next = students.filter(item => item.id !== student.id);
        await saveStudents(next);
      } },
    ]);
  };

  const calculateGrade = (score) => {
    if (score === null || score === undefined) return { grade: 'N/A', remark: 'Pending' };
    if (score >= 70) return { grade: 'A', remark: 'Excellent' };
    if (score >= 60) return { grade: 'B', remark: 'Very Good' };
    if (score >= 50) return { grade: 'C', remark: 'Good' };
    if (score >= 45) return { grade: 'D', remark: 'Pass' };
    if (score >= 40) return { grade: 'E', remark: 'Fair' };
    return { grade: 'F', remark: 'Fail' };
  };

  const handleLogin = () => {
    if (portalMode === 'admin') {
      if (password === ADMIN_PASSWORD) setCurrentUser({ role: 'admin' });
      else Alert.alert('Access denied', 'Incorrect Admin Master Password.');
      return;
    }

    const student = students.find(s =>
      s.id.toLowerCase() === loginId.trim().toLowerCase() &&
      s.surname.toLowerCase() === password.trim().toLowerCase()
    );
    if (student) setCurrentUser({ role: 'student', data: student });
    else Alert.alert('Login error', 'Invalid Student ID or Surname password.');
  };

  const logout = () => {
    setCurrentUser(null);
    setLoginId('');
    setPassword('');
  };

  if (!currentUser) {
    return (
      <SafeAreaView style={styles.container}>
        <View style={styles.card}>
          <Text style={styles.title}>Bombi High School</Text>
          <Text style={styles.subtitle}>{portalMode === 'admin' ? 'Admin Control Portal' : 'Student Record Portal'}</Text>
          <View style={styles.tabContainer}>
            <TouchableOpacity style={[styles.tab, portalMode === 'student' && styles.activeTab]} onPress={() => setPortalMode('student')}><Text style={styles.tabText}>Student</Text></TouchableOpacity>
            <TouchableOpacity style={[styles.tab, portalMode === 'admin' && styles.activeTab]} onPress={() => setPortalMode('admin')}><Text style={styles.tabText}>Admin</Text></TouchableOpacity>
          </View>
          {portalMode === 'student' && <TextInput style={styles.input} placeholder="Student ID (e.g. BHS-2026-001)" placeholderTextColor="#94a3b8" value={loginId} onChangeText={setLoginId} autoCapitalize="characters" />}
          <TextInput style={styles.input} placeholder={portalMode === 'admin' ? 'Master Admin Password' : 'Password (Surname)'} placeholderTextColor="#94a3b8" secureTextEntry value={password} onChangeText={setPassword} />
          <TouchableOpacity style={styles.button} onPress={handleLogin}><Text style={styles.buttonText}>{portalMode === 'admin' ? 'Unlock Database' : 'Enter Portal'}</Text></TouchableOpacity>
          {storageError && <Text style={styles.errorText}>{storageError}</Text>}
        </View>
      </SafeAreaView>
    );
  }

  if (currentUser.role === 'student') {
    const s = students.find(student => student.id === currentUser.data.id) || currentUser.data;
    return (
      <SafeAreaView style={styles.container}>
        <View style={styles.header}>
          <Text style={styles.nameText}>{s.fullName}</Text>
          <Text style={styles.subText}>{s.id} | {s.department}</Text>
          <Text style={styles.subText}>Class: {s.classLevel} {s.classArm}</Text>
          <TouchableOpacity onPress={logout} style={styles.logoutBtn}><Text style={styles.logoutText}>Logout</Text></TouchableOpacity>
        </View>
        <Text style={styles.sectionTitle}>Academic Report Sheet</Text>
        <FlatList data={s.subjects} keyExtractor={item => item} renderItem={({ item }) => {
          const score = s.scores?.[item]; const result = calculateGrade(score);
          return <View style={styles.scoreCard}><View><Text style={styles.subjectName}>{item}</Text><Text style={styles.remarkText}>{result.remark}</Text></View><View style={styles.scoreRight}><Text style={styles.scoreVal}>{score !== undefined ? `${score}%` : 'N/A'}</Text><Text style={styles.gradeBadge}>Grade {result.grade}</Text></View></View>;
        }} ListEmptyComponent={<Text style={styles.emptyText}>No subjects have been assigned yet.</Text>} />
      </SafeAreaView>
    );
  }

  return (
    <SafeAreaView style={styles.container}>
      <View style={styles.header}>
        <Text style={styles.nameText}>Admin Control Center</Text>
        <Text style={styles.subText}>Managing {students.length} Enrolled Students</Text>
        <View style={styles.actionRow}>
          <TouchableOpacity style={styles.smallButton} onPress={openAddStudent}><Text style={styles.buttonText}>+ Register Student</Text></TouchableOpacity>
          <TouchableOpacity onPress={logout} style={styles.logoutBtn}><Text style={styles.logoutText}>Logout</Text></TouchableOpacity>
        </View>
      </View>

      {storageError && <View style={styles.errorBanner}><Text style={styles.errorText}>{storageError}</Text><TouchableOpacity onPress={loadData}><Text style={styles.retryText}>Retry</Text></TouchableOpacity></View>}
      <Text style={styles.sectionTitle}>Student Directory</Text>
      <FlatList data={students} keyExtractor={item => item.id} renderItem={({ item }) => (
        <View style={styles.studentRow}>
          <View style={{ flex: 1 }}><Text style={styles.studentRowName}>{item.fullName}</Text><Text style={styles.studentRowSub}>{item.id} • {item.classLevel} {item.classArm}</Text></View>
          <Text style={styles.deptBadge}>{item.department}</Text>
          <TouchableOpacity onPress={() => openEditStudent(item)} style={styles.rowAction}><Text style={styles.actionText}>Edit</Text></TouchableOpacity>
          <TouchableOpacity onPress={() => deleteStudent(item)} style={styles.rowAction}><Text style={styles.deleteText}>Delete</Text></TouchableOpacity>
        </View>
      )} ListEmptyComponent={<Text style={styles.emptyText}>No students registered. Use “Register Student” to add one.</Text>} />

      <Modal visible={formVisible} animationType="slide" transparent onRequestClose={() => setFormVisible(false)}>
        <KeyboardAvoidingView style={styles.modalBackdrop} behavior={Platform.OS === 'ios' ? 'padding' : undefined}>
          <View style={styles.modalCard}>
            <ScrollView keyboardShouldPersistTaps="handled">
              <Text style={styles.modalTitle}>{editingStudent ? 'Edit student' : 'Register student'}</Text>
              {!editingStudent && <Text style={styles.modalHint}>A BHS student ID will be generated automatically.</Text>}
              {Object.entries({ surname: 'Surname', fullName: 'Full name', department: 'Department', classLevel: 'Class', classArm: 'Class arm' }).map(([key, label]) => (
                <TextInput key={key} style={styles.input} placeholder={label} placeholderTextColor="#94a3b8" value={form[key]} onChangeText={value => setForm(current => ({ ...current, [key]: value }))} />
              ))}
              <View style={styles.actionRow}><TouchableOpacity style={styles.button} onPress={saveStudent}><Text style={styles.buttonText}>{editingStudent ? 'Save changes' : 'Register student'}</Text></TouchableOpacity><TouchableOpacity style={styles.cancelButton} onPress={() => setFormVisible(false)}><Text style={styles.buttonText}>Cancel</Text></TouchableOpacity></View>
            </ScrollView>
          </View>
        </KeyboardAvoidingView>
      </Modal>
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: '#0f172a', paddingHorizontal: 16, paddingTop: 40 },
  card: { backgroundColor: '#1e293b', padding: 24, borderRadius: 16, marginTop: 40 },
  title: { color: '#ffffff', fontSize: 24, fontWeight: 'bold', textAlign: 'center' },
  subtitle: { color: '#818cf8', fontSize: 13, textAlign: 'center', marginBottom: 20 },
  tabContainer: { flexDirection: 'row', backgroundColor: '#0f172a', borderRadius: 8, marginBottom: 16, padding: 4 },
  tab: { flex: 1, paddingVertical: 8, alignItems: 'center', borderRadius: 6 },
  activeTab: { backgroundColor: '#4f46e5' },
  tabText: { color: '#ffffff', fontWeight: 'bold', fontSize: 12 },
  input: { backgroundColor: '#0f172a', color: '#ffffff', padding: 12, borderRadius: 8, marginBottom: 12, borderBottomWidth: 1, borderBottomColor: '#334155' },
  button: { backgroundColor: '#4f46e5', padding: 14, borderRadius: 8, alignItems: 'center', marginTop: 8, flex: 1 },
  smallButton: { backgroundColor: '#4f46e5', paddingHorizontal: 14, paddingVertical: 10, borderRadius: 8 },
  cancelButton: { backgroundColor: '#334155', padding: 14, borderRadius: 8, alignItems: 'center', marginTop: 8, flex: 1 },
  buttonText: { color: '#ffffff', fontWeight: 'bold' },
  header: { backgroundColor: '#1e293b', padding: 16, borderRadius: 12, marginBottom: 16 },
  nameText: { color: '#ffffff', fontSize: 18, fontWeight: 'bold' },
  subText: { color: '#94a3b8', fontSize: 12, marginTop: 2 },
  logoutBtn: { marginTop: 12, alignSelf: 'flex-start' },
  logoutText: { color: '#fb7185', fontWeight: 'bold', fontSize: 12 },
  actionRow: { flexDirection: 'row', gap: 10, alignItems: 'center', marginTop: 10 },
  sectionTitle: { color: '#ffffff', fontSize: 16, fontWeight: 'bold', marginBottom: 12 },
  scoreCard: { backgroundColor: '#1e293b', padding: 14, borderRadius: 10, flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center', marginBottom: 8 },
  subjectName: { color: '#ffffff', fontWeight: 'bold', fontSize: 14 },
  remarkText: { color: '#94a3b8', fontSize: 11 },
  scoreRight: { alignItems: 'flex-end' },
  scoreVal: { color: '#818cf8', fontWeight: 'bold', fontSize: 16 },
  gradeBadge: { color: '#34d399', fontSize: 11, fontWeight: 'bold' },
  studentRow: { backgroundColor: '#1e293b', padding: 14, borderRadius: 10, flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center', marginBottom: 8, gap: 8 },
  studentRowName: { color: '#ffffff', fontWeight: 'bold', fontSize: 14 },
  studentRowSub: { color: '#94a3b8', fontSize: 11 },
  deptBadge: { color: '#818cf8', fontSize: 11, fontWeight: 'bold', backgroundColor: '#0f172a', paddingHorizontal: 8, paddingVertical: 4, borderRadius: 4 },
  rowAction: { paddingHorizontal: 6, paddingVertical: 8 },
  actionText: { color: '#818cf8', fontWeight: 'bold' },
  deleteText: { color: '#fb7185', fontWeight: 'bold' },
  errorText: { color: '#fda4af', fontSize: 12 },
  errorBanner: { backgroundColor: '#3f1d2b', borderRadius: 10, padding: 12, marginBottom: 12, flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center' },
  retryText: { color: '#ffffff', fontWeight: 'bold', marginLeft: 12 },
  emptyText: { color: '#94a3b8', textAlign: 'center', padding: 24 },
  modalBackdrop: { flex: 1, backgroundColor: 'rgba(0,0,0,0.65)', justifyContent: 'flex-end' },
  modalCard: { backgroundColor: '#1e293b', borderTopLeftRadius: 20, borderTopRightRadius: 20, padding: 20, maxHeight: '85%' },
  modalTitle: { color: '#ffffff', fontSize: 22, fontWeight: 'bold', marginBottom: 6 },
  modalHint: { color: '#94a3b8', fontSize: 12, marginBottom: 16 },
});
