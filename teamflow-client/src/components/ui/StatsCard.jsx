function StatsCard({ title, value }) {
  return (
    <div className="stats-card">
      <span>{title}</span>
      <strong>{value}</strong>
    </div>
  );
}

export default StatsCard;
