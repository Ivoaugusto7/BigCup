function Layout({ children }) {
  return (
    <div className="min-h-screen bg-background text-slate-100">
      <main className="mx-auto flex min-h-screen max-w-6xl items-center justify-center px-6 py-16 sm:px-8 lg:px-10">
        {children}
      </main>
    </div>
  );
}

export default Layout;
